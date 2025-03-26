using Newtonsoft.Json;

namespace WebADS.Models.MoySklad.Stock
{
    public class Meta
    {
        [JsonProperty("href")]
        public string Href { get; set; } = string.Empty;

        [JsonProperty("metadataHref")]
        public string MetadataHref { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mediaType")]
        public string MediaType { get; set; } = string.Empty;

        [JsonProperty("size")]
        public int? Size { get; set; }

        [JsonProperty("limit")]
        public int? Limit { get; set; }

        [JsonProperty("offset")]
        public int? Offset { get; set; }

        [JsonProperty("nextHref")]
        public string NextHref { get; set; } = string.Empty;
    }

    public class Employee
    {
        [JsonProperty("href")]
        public string Href { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mediaType")]
        public string MediaType { get; set; } = string.Empty;
    }

    public class Context
    {
        [JsonProperty("employee")]
        public Employee? Employee { get; set; }
    }

    public class StockByStore
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("stock")]
        public double Stock { get; set; }

        [JsonProperty("reserve")]
        public double Reserve { get; set; }

        [JsonProperty("inTransit")]
        public double InTransit { get; set; }
    }

    public class Row
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }

        [JsonProperty("stockByStore")]
        public List<StockByStore>? StockByStore { get; set; }
    }

    public class StockByStoreRoot
    {
        [JsonProperty("context")]
        public Context? Context { get; set; }

        [JsonProperty("meta")]
        public Meta? Meta { get; set; }

        [JsonProperty("rows")]
        public List<Row>? Rows { get; set; }
    }
}
