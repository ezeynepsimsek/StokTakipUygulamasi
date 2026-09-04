using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokTakipUygulamasi.Models;
using System.Linq; // Grafikteki Liste işlemleri için eklendi

namespace StokTakipUygulamasi.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Özet Rakamları Hesaplama
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();
            
            // SADECE İPTAL OLMAYAN siparişleri sayıyoruz
            ViewBag.TotalOrders = await _context.Orders.CountAsync(o => o.Status != OrderStatus.Iptal);
            
            // SADECE İPTAL OLMAYAN siparişlerin tutarlarını topluyoruz
            // (OrderDetails üzerinden gitmek yerine direkt hesaplanmış TotalAmount'u kullanıyoruz)
            var totalSales = await _context.Orders
                .Where(o => o.Status != OrderStatus.Iptal)
                .SumAsync(o => o.TotalAmount);
            ViewBag.TotalSales = totalSales;

            // 2. Düşük Stoklu Ürünler (Stok miktarı 10'dan az olan en kritik 5 ürün)
            var lowStockProducts = await _context.Products
                .Where(p => p.StockQuantity < 10)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToListAsync();
            ViewBag.LowStockProducts = lowStockProducts;

            // 3. Grafik için Son 7 Günün Satış (Sipariş) Sayıları
            var last7Days = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-i)).Reverse().ToList();
            var chartLabels = last7Days.Select(d => d.ToString("dd MMM")).ToList();
            var chartData = new List<int>();

            foreach (var date in last7Days)
            {
                // Grafikte de yine sadece geçerli (İptal edilmemiş) siparişlerin istatistiğini tutuyoruz
                var orderCount = await _context.Orders
                    .CountAsync(o => o.OrderDate.Date == date && o.Status != OrderStatus.Iptal);
                chartData.Add(orderCount);
            }

            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartData = chartData;

            return View();
        }
    }
}