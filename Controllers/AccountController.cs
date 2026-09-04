using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace StokTakipUygulamasi.Controllers
{
    public class AccountController : Controller
    {
        // 1. Giriş Ekranını Aç
        public IActionResult Login()
        {
            return View();
        }

        // 2. Giriş Yap Butonuna Basıldığında
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Basit bir şifre kontrolü (Bunu ileride veritabanına bağlayabiliriz)
            if (username == "admin" && password == "12345")
            {
                // Kullanıcıya bir kimlik kartı (Claim) oluşturuyoruz
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Tarayıcıya güvenli çerezi (Cookie) bırakıp giriş yaptırıyoruz
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home"); // Başarılıysa Ana Sayfaya yönlendir
            }

            // Hatalıysa uyarı ver
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
            return View();
        }

        // 3. Çıkış Yap
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}