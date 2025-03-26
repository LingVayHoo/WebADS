namespace WebADS.Models
{
    public class ArticleProfitability
    {
        public string ProductID { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public double AMS { get; set; }
        public double MaxSales { get; set; }
        public double SoldLastInterval { get; set; }
        public double NeedToOrder { get; set; } = -999;
        public double Stock { get; set; }
    }
}
