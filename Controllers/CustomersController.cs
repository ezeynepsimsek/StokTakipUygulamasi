using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokTakipUygulamasi.Models;
using Microsoft.AspNetCore.Authorization;

namespace StokTakipUygulamasi.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Müşterileri Listeleme (Arama ve Sayfalama)
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            var customers = _context.Customers.AsQueryable();

            // ARAMA: İsim, Soyisim, Email veya Telefonda arama yap
            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c => c.FirstName.Contains(searchString) || 
                                                 c.LastName.Contains(searchString) ||
                                                 c.Email.Contains(searchString) ||
                                                 c.Phone.Contains(searchString));
            }

            // SAYFALAMA
            int pageSize = 5; 
            int pageNumber = page ?? 1; 
            int totalItems = await customers.CountAsync(); 
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var paginatedCustomers = await customers
                .OrderByDescending(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            return View(paginatedCustomers);
        }

        // Yeni Müşteri Ekleme Ekranı
        public IActionResult Create()
        {
            return View();
        }

        // Yeni Müşteriyi Kaydetme
        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // 4. Müşteri Düzenleme Ekranını Açma
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // 5. Düzenlenen Müşteriyi Veritabanında Güncelleme
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // 6. Müşteri Silme Onay Ekranını Açma
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // 7. Müşteriyi, Siparişlerini Silme ve Ürünleri Stoğa İade Etme
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                // 1. ADIM: Müşterinin siparişlerini ve o siparişlerin İÇİNDEKİ ÜRÜNLERİ (OrderDetails) getir
                var customerOrders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .Where(o => o.CustomerId == id)
                    .ToListAsync();

                if (customerOrders.Any())
                {
                    // 2. ADIM: İptal olan her siparişin içindeki ürün adedini stoğa geri ekle
                    foreach (var order in customerOrders)
                    {
                        if (order.OrderDetails != null)
                        {
                            foreach (var detail in order.OrderDetails)
                            {
                                var product = await _context.Products.FindAsync(detail.ProductId);
                                if (product != null)
                                {
                                    // Satılan ürünü rafa geri koyuyoruz
                                    product.StockQuantity += detail.Quantity;
                                    _context.Update(product);
                                }
                            }
                        }
                    }
                    // 3. ADIM: Siparişleri sil
                    _context.Orders.RemoveRange(customerOrders);
                }

                // 4. ADIM: En son müşterinin kendisini sil
                _context.Customers.Remove(customer);
                
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}