using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StokTakipUygulamasi.Models;

namespace StokTakipUygulamasi.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Veritabanı bağlantımızı içeri alıyoruz
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Kategorileri Listeleme Ekranı
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        // 2. Yeni Kategori Ekleme Ekranını Açma
        public IActionResult Create()
        {
            return View();
        }

        // 3. Yeni Kategoriyi Veritabanına Kaydetme
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync(); // SQL'e kaydet
                return RedirectToAction(nameof(Index)); // Listeye geri dön
            }
            return View(category);
        }

        // 4. Kategori Düzenleme Ekranını Açma
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // 5. Düzenlenen Kategoriyi Veritabanında Güncelleme
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // 6. Kategori Silme Onay Ekranını Açma
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // 7. Kategoriyi Kalıcı Olarak Silme İşlemi
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}