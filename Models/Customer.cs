namespace StokTakipUygulamasi.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty; // Ad
        public string LastName { get; set; } = string.Empty;  // Soyad
        public string Phone { get; set; } = string.Empty;     // Telefon
        public string Email { get; set; } = string.Empty;     // E-Posta

        // EF Core İlişkisi: Bir müşterinin birden fazla siparişi olabilir
        public List<Order>? Orders { get; set; }
    }
}