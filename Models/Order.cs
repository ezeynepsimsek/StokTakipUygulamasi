namespace StokTakipUygulamasi.Models
{
    // Sipariş durumlarını burada tanımlıyoruz
    public enum OrderStatus
    {
        Bekliyor,
        Hazirlaniyor,
        Tamamlandi,
        Iptal
    }

    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now; 
        public decimal TotalAmount { get; set; } 
        
        // SİPARİŞ DURUMU (Sadece 1 tane olmalı)
        public OrderStatus Status { get; set; } = OrderStatus.Bekliyor; 

        // Hangi müşteri sipariş verdi? (Foreign Key)
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // Siparişin içindeki ürün kalemleri listesi
        public List<OrderDetail>? OrderDetails { get; set; }
    }
}