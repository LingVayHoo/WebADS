namespace WebADS.Models
{
    public class ArticleParameters
    {
        public Guid Id { get; set; }
        public string ProductID { get; set; } = string.Empty;
        public float AWS {  get; set; }
        public string SalesMethod {  get; set; } = string.Empty;
        public float MinSalesQty {  get; set; }
        public float MultipackQty {  get; set; }
        public float PalletQty { get; set; }
    }
}
