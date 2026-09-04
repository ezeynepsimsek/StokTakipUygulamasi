namespace StokTakipUygulamasi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Hata/Uyarı çözüldü
        public string? Description { get; set; } 
        public decimal Price { get; set; } 
        public int StockQuantity { get; set; } 

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}