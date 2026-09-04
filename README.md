ASP.NET Core MVC Stok, Sipariş ve Müşteri Yönetimi Sistemi

Bu proje, küçük ve orta ölçekli işletmelerin (KOBİ) depo, stok, finans ve müşteri ilişkileri süreçlerini merkezi bir dijital platform üzerinden yönetebilmeleri amacıyla geliştirilmiş kapsamlı bir web otomasyonudur. Modern yazılım mühendisliği prensipleri gözetilerek, test edilebilir ve kurumsal ölçekte kullanıma hazır bir mimari inşa edilmiştir.


-ÖNE ÇIKAN ÖZELLİKLER

Akıllı Sipariş ve Sepet Yönetimi: Sepete ürün eklenirken anlık veritabanı stok kontrolü yapılır. Yetersiz stok durumunda işlem durdurulur. Sipariş tamamlandığında stoklar eşzamanlı olarak güncellenir.

Gelişmiş Durum Takibi (Enum): Siparişler Bekliyor, Hazırlanıyor, Tamamlandı ve İptal durumlarında takip edilir. Bir sipariş "İptal" edildiğinde, içerisindeki satılan ürün adetleri otomatik olarak ana depoya iade edilir. Çift iadeyi önleyen koruma algoritmaları mevcuttur.

Özel "Basamaklı Stok İade" Algoritması: Müşteri silme işlemlerinde standart Cascade Delete davranışı ezilerek özel bir iş mantığı yazılmıştır. Silinen müşterinin geçmiş siparişleri ve sepetlerindeki ürün adetleri tespit edilip depoya iade edildikten sonra kayıtlar güvenlice temizlenir.

Dinamik Yönetim Paneli (Dashboard): Asenkron metotlar ve toplulaştırma sorguları ile toplam ciro, aktif sipariş sayısı ve kritik seviyedeki (10 adet altı) ürünler anlık listelenir. Chart.js ile son 7 günün kesinleşmiş satış grafiği çizdirilir (İptal edilen işlemler muhasebeyi yanıltmamak için filtrelenir).

Güvenlik ve Performans: [Authorize] ile çerez bazlı (Cookie Authentication) yetkilendirme sistemi kurgulanmıştır. Büyük veriler için C# LINQ Skip/Take metotlarıyla özel sayfalama (Pagination) ve arama motoru geliştirilmiştir.

Excel'e Dışa Aktar: Veritabanındaki operasyonel kayıtlar StringBuilder ve UTF-8 BOM etiketleri kullanılarak Türkçe karakter sorunu olmadan CSV/Excel formatında raporlanabilir.


-KULLANILAN TEKNOLOJİLER

Backend: C#, ASP.NET Core MVC, LINQ

Veritabanı & ORM: MS SQL Server, Entity Framework Core (Code-First)

Frontend: HTML5, CSS3, Tailwind CSS (Utility-first), JavaScript

Veri Görselleştirme: Chart.js


-KURULUM ADIMLARI

Projeyi kendi bilgisayarınızda (lokal ortamda) çalıştırmak için aşağıdaki adımları izleyebilirsiniz:

Repoyu Klonlayın:
git clone https://github.com/KULLANICI_ADINIZ/aspnet-core-inventory-management.git

Veritabanı Bağlantısını Ayarlayın:
appsettings.json dosyasını açın ve DefaultConnection dizgesini kendi SQL Server yapılandırmanıza göre düzenleyin.

Veritabanını Oluşturun (Migration):
Visual Studio'da Package Manager Console'u (PMC) açın ve veritabanı tablolarını oluşturmak için aşağıdaki komutu çalıştırın:
Update-Database

Projeyi Çalıştırın:
Projeyi Visual Studio üzerinden başlatın (F5 veya Ctrl+F5) ve giriş ekranından sisteme erişim sağlayın.


-LİSANS

Bu proje, açık kaynak kodlu olarak geliştirilmiştir ve eğitim/portföy amaçlıdır.
