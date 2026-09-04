namespace StokTakipUygulamasi.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Hata/Uyarı buradaki '= string.Empty;' ile çözüldü

        public List<Product>? Products { get; set; }
    }
}