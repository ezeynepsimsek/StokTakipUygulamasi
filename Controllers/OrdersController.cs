using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokTakipUygulamasi.Models; // Kendi proje adına göre gerekirse burayı düzelt
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StokTakipUygulamasi.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LİSTELEME, ARAMA VE SAYFALAMA
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            // --- GEÇMİŞ SİPARİŞLERİ OTOMATİK KURTARMA KODU ---
            var hataliSiparisler = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.TotalAmount == 0 && o.OrderDetails.Any())
                .ToListAsync();

            foreach (var siparis in hataliSiparisler)
            {
                // İçindeki ürünlerin fiyat ve adetlerini çarpıp asıl tutarı buluyoruz
                siparis.TotalAmount = siparis.OrderDetails.Sum(od => od.Quantity * od.UnitPrice);
            }
            
            if (hataliSiparisler.Any()) 
            {
                await _context.SaveChangesAsync(); // Doğru tutarları veritabanına kalıcı olarak kaydet
            }

            var orders = _context.Orders.Include(o => o.Customer).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.Id.ToString() == searchString ||
                                           o.Customer.FirstName.Contains(searchString) ||
                                           o.Customer.LastName.Contains(searchString));
            }

            int pageSize = 5; 
            int pageNumber = page ?? 1; 
            int totalItems = await orders.CountAsync(); 
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var paginatedOrders = await orders
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(paginatedOrders);
        }

        // 2. YENİ SİPARİŞ OLUŞTURMA (Müşteri Seçimi)
        public IActionResult Create()
        {
            var customerList = _context.Customers.Select(c => new 
            {
                Id = c.Id,
                FullName = c.FirstName + " " + c.LastName
            }).ToList();

            ViewBag.CustomerId = new SelectList(customerList, "Id", "FullName");
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int customerId)
        {
            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Bekliyor, // Enum kullandık
                TotalAmount = 0
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        // 3. SİPARİŞ DETAY SAYFASI
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            ViewBag.Products = _context.Products.Where(p => p.StockQuantity > 0).ToList();
            return View(order);
        }

        // 4. SEPETE ÜRÜN EKLEME VE STOKTAN DÜŞME
        [HttpPost]
        public async Task<IActionResult> AddProductToOrder(int OrderId, int ProductId, int Quantity)
        {
            var product = await _context.Products.FindAsync(ProductId);
            if (product == null || product.StockQuantity < Quantity)
            {
                TempData["ErrorMessage"] = "Yetersiz stok!";
                return RedirectToAction(nameof(Details), new { id = OrderId });
            }

            var orderDetail = new OrderDetail
            {
                OrderId = OrderId,
                ProductId = ProductId,
                Quantity = Quantity,
                UnitPrice = product.Price
            };

            product.StockQuantity -= Quantity;
            _context.OrderDetails.Add(orderDetail);
            
            // TotalAmount Güncellemesi
            var order = await _context.Orders.FindAsync(OrderId);
            order.TotalAmount += (Quantity * product.Price);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Ürün sepete eklendi.";
            return RedirectToAction(nameof(Details), new { id = OrderId });
        }

        // 5. SEPETTEN ÜRÜN SİLME VE STOK İADE
        [HttpPost]
        public async Task<IActionResult> RemoveProductFromOrder(int OrderId, int ProductId)
        {
            var orderDetail = await _context.OrderDetails
                .FirstOrDefaultAsync(od => od.OrderId == OrderId && od.ProductId == ProductId);

            if (orderDetail != null)
            {
                var product = await _context.Products.FindAsync(ProductId);
                if (product != null)
                {
                    product.StockQuantity += orderDetail.Quantity;
                }

                // TotalAmount Güncellemesi
                var order = await _context.Orders.FindAsync(OrderId);
                if (order != null) {
                     order.TotalAmount -= (orderDetail.Quantity * orderDetail.UnitPrice);
                }

                _context.OrderDetails.Remove(orderDetail);
                await _context.SaveChangesAsync();

                var remainingItems = await _context.OrderDetails.AnyAsync(od => od.OrderId == OrderId);
                if (!remainingItems)
                {
                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "İçinde ürün kalmadığı için sipariş iptal edildi.";
                    return RedirectToAction(nameof(Index)); 
                }
                TempData["SuccessMessage"] = "Ürün siparişten çıkarıldı, stok iade edildi.";
            }
            return RedirectToAction(nameof(Details), new { id = OrderId });
        }

        // 6. SİPARİŞ DURUMUNU GÜNCELLEME VE OTOMATİK STOK İADE
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, OrderStatus newStatus)
        {
            // Stok iadesi yapabilmek için siparişi içindeki ürün detaylarıyla birlikte çekiyoruz
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                // Eğer yeni durum "İptal" ise ve sipariş DAHA ÖNCE iptal edilmemişse (çift iadeyi önlemek için)
                if (newStatus == OrderStatus.Iptal && order.Status != OrderStatus.Iptal)
                {
                    // Siparişteki her bir ürünü bulup, satılan adedi ana stoğa geri ekliyoruz
                    foreach (var detail in order.OrderDetails)
                    {
                        var product = await _context.Products.FindAsync(detail.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += detail.Quantity;
                        }
                    }
                }

                order.Status = newStatus;
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Sipariş durumu güncellendi. İptal işlemi yapıldıysa stoklar depoya iade edildi.";
            }
            
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // 7. EXCEL RAPORU
        public async Task<IActionResult> ExportToExcel()
        {
            var orders = await _context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails).OrderByDescending(o => o.OrderDate).ToListAsync();
            var builder = new StringBuilder();
            builder.AppendLine("Sipariş No;Tarih;Müşteri;Toplam Tutar (TL);Durum");

            foreach (var item in orders)
            {
                var customerName = item.Customer != null ? $"{item.Customer.FirstName} {item.Customer.LastName}" : "-";
                builder.AppendLine($"{item.Id};{item.OrderDate:dd.MM.yyyy HH:mm};{customerName};{item.TotalAmount};{item.Status}");
            }

            var bom = new byte[] { 0xEF, 0xBB, 0xBF }; 
            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return File(bom.Concat(bytes).ToArray(), "text/csv", "Satis_Raporu.csv");
        }
    }
}