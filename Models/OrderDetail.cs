namespace StokTakipUygulamasi.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int Quantity { get; set; } // Üründen kaç adet alındı?
        
        // Satış anındaki fiyatı buraya kaydederiz ki, yarın ürünün fiyatına 
        // zam gelirse geçmiş siparişlerin toplam tutarı bozulmasın.
        public decimal UnitPrice { get; set; } 

        // Hangi siparişin detayı?
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Hangi ürün satıldı?
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}