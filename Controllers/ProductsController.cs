using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StokTakipUygulamasi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace StokTakipUygulamasi.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Ürünleri Listeleme (Arama ve Sayfalama Eklenmiş Halde)
        public async Task<IActionResult> Index(string searchString, int? page)
        {
            // Veritabanı sorgusunu hemen çalıştırmadan (AsQueryable) hazırlıyoruz
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            // ARAMA BÖLÜMÜ: Eğer arama kutusuna bir şey yazılmışsa
            if (!string.IsNullOrEmpty(searchString))
            {
                // Ürün adında VEYA Kategori adında o kelimeyi ara
                products = products.Where(p => p.Name.Contains(searchString) || 
                                               p.Category.Name.Contains(searchString));
            }

            // SAYFALAMA BÖLÜMÜ
            int pageSize = 5; // Her sayfada kaç ürün gösterilecek? (Test için 5 yaptık)
            int pageNumber = page ?? 1; // Sayfa parametresi boşsa 1. sayfa kabul et
            
            // Toplam kaç ürün olduğunu bul ve toplam sayfa sayısını hesapla
            int totalItems = await products.CountAsync(); 
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Sadece istenen sayfanın ürünlerini çek (Skip: Atla, Take: Al)
            var paginatedProducts = await products
                .OrderByDescending(p => p.Id) // En son eklenen en üstte olsun
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // View'a gerekli bilgileri gönderiyoruz
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString; // Sayfa değiştiğinde arama kutusu dolu kalsın diye

            return View(paginatedProducts);
        }

        // 2. Yeni Ürün Ekleme Ekranını Açma
        public IActionResult Create()
        {
            // Kullanıcıya formda kategori seçtirmek için kategorileri ViewBag ile yolluyoruz
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // 3. Yeni Ürünü Veritabanına Kaydetme
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Hata olursa kategorileri tekrar doldurup formu geri göster
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 4. Ürün Düzenleme Ekranını Açma
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Kategori listesini dropdown için tekrar dolduruyoruz, mevcut kategorisi seçili gelsin
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 5. Düzenlenen Ürünü Veritabanında Güncelleme
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Hata olursa dropdown'ı tekrar doldur
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 6. Ürün Silme Onay Ekranını Açma
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // Silinecek ürünü kategorisiyle birlikte buluyoruz ki ekranda detayını gösterelim
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // 7. Ürünü Veritabanından Kalıcı Olarak Silme İşlemi
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 8. Ürünleri Excel (CSV) Olarak İndirme
        public async Task<IActionResult> ExportToExcel()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();

            var builder = new StringBuilder();
            
            // Başlık satırı (Sütunlar noktalı virgül ile ayrılır)
            builder.AppendLine("ID;Ürün Adı;Kategori;Fiyat;Stok Miktarı");

            // Tüm ürünleri dönüp alt alta ekliyoruz
            foreach (var item in products)
            {
                var categoryName = item.Category != null ? item.Category.Name : "-";
                // Fiyatlardaki virgüller karışmasın diye formatlıyoruz
                builder.AppendLine($"{item.Id};{item.Name};{categoryName};{item.Price};{item.StockQuantity}");
            }

            // Excel'in Türkçe karakterleri (Ş, Ç, Ğ vb.) düzgün okuması için UTF-8 BOM ekliyoruz (Çok kritik bir detaydır!)
            var bom = new byte[] { 0xEF, 0xBB, 0xBF }; 
            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            var result = bom.Concat(bytes).ToArray();

            // Dosyayı kullanıcıya indirt
            return File(result, "text/csv", "Urun_Stok_Raporu.csv");
        }

    }
    
}