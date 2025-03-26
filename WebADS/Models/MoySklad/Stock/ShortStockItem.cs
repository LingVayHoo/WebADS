namespace WebADS.Models.MoySklad.Stock
{
    public class ShortStockItem
    {
        public string AssortmentId { get; set; } = string.Empty;
        public string? StoreId { get; set; } // StoreId может быть null
        public float Quantity { get; set; }
    }
}
