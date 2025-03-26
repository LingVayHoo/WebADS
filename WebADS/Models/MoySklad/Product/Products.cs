using Newtonsoft.Json;

namespace WebADS.Models.MoySklad.Product
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

        [JsonProperty("nextHref")] 
        public string? NextHref { get; set; }
    }

    public class EmployeeMeta
    {
        [JsonProperty("href")]
        public string Href { get; set; } = string.Empty;

        [JsonProperty("metadataHref")]
        public string MetadataHref { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mediaType")]
        public string MediaType { get; set; } = string.Empty;
    }

    public class Employee
    {
        [JsonProperty("meta")]
        public EmployeeMeta? Meta { get; set; }
    }

    public class Context
    {
        [JsonProperty("employee")]
        public Employee? Employee { get; set; }
    }

    public class GroupMeta
    {
        [JsonProperty("href")]
        public string Href { get; set; } = string.Empty;

        [JsonProperty("metadataHref")]
        public string MetadataHref { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mediaType")]
        public string MediaType { get; set; } = string.Empty;
    }

    public class Group
    {
        [JsonProperty("meta")]
        public GroupMeta? Meta { get; set; }
    }

    public class ProductFolderMeta
    {
        [JsonProperty("href")]
        public string Href { get; set; } = string.Empty;

        [JsonProperty("metadataHref")]
        public string MetadataHref { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mediaType")]
        public string MediaType { get; set; } = string.Empty;
    }

    public class ProductFolder
    {
        [JsonProperty("meta")]
        public ProductFolderMeta? Meta { get; set; }
    }

    public class UomMeta
    {
        [JsonProperty("href")]
        public string Href { get; set; } = string.Empty;

        [JsonProperty("metadataHref")]
        public string MetadataHref { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mediaType")]
        public string MediaType { get; set; } = string.Empty;
    }

    public class Uom
    {
        [JsonProperty("meta")]
        public UomMeta? Meta { get; set; }
    }

    public class ImagesMeta
    {
        [JsonProperty("href")]
        public string Href { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mediaType")]
        public string MediaType { get; set; } = string.Empty;

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }
    }

    public class Images
    {
        [JsonProperty("meta")]
        public ImagesMeta? Meta { get; set; }
    }

    public class CurrencyMeta
    {
        [JsonProperty("href")]
        public string Href { get; set; } = string.Empty;

        [JsonProperty("metadataHref")]
        public string MetadataHref { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mediaType")]
        public string MediaType { get; set; } = string.Empty;
    }

    public class Currency
    {
        [JsonProperty("meta")]
        public CurrencyMeta? Meta { get; set; }
    }

    public class MinPrice
    {
        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("currency")]
        public Currency? Currency { get; set; }
    }

    public class PriceTypeMeta
    {
        [JsonProperty("href")]
        public string Href { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mediaType")]
        public string MediaType { get; set; } = string.Empty;
    }

    public class PriceType
    {
        [JsonProperty("meta")]
        public PriceTypeMeta? Meta { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("externalCode")]
        public string ExternalCode { get; set; } = string.Empty;
    }

    public class SalePrice
    {
        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("currency")]
        public Currency? Currency { get; set; }

        [JsonProperty("priceType")]
        public PriceType? PriceType { get; set; }
    }

    public class BuyPrice
    {
        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("currency")]
        public Currency? Currency { get; set; }
    }

    public class Barcode
    {
        [JsonProperty("ean13")]
        public string Ean13 { get; set; } = string.Empty;

        [JsonProperty("code128")]
        public string Code128 { get; set; } = string.Empty;
    }

    public class FilesMeta
    {
        [JsonProperty("href")]
        public string Href { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mediaType")]
        public string MediaType { get; set; } = string.Empty;

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }
    }

    public class Files
    {
        [JsonProperty("meta")]
        public FilesMeta? Meta { get; set; }
    }

    public class Product
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("accountId")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("shared")]
        public bool Shared { get; set; }

        [JsonProperty("group")]
        public Group? Group { get; set; }

        [JsonProperty("updated")]
        public DateTime Updated { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;

        [JsonProperty("externalCode")]
        public string ExternalCode { get; set; } = string.Empty;

        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("pathName")]
        public string PathName { get; set; } = string.Empty;

        [JsonProperty("productFolder")]
        public ProductFolder? ProductFolder { get; set; }

        [JsonProperty("useParentVat")]
        public bool UseParentVat { get; set; }

        [JsonProperty("uom")]
        public Uom? Uom { get; set; }

        [JsonProperty("images")]
        public Images? Images { get; set; }

        [JsonProperty("minPrice")]
        public MinPrice? MinPrice { get; set; }

        [JsonProperty("salePrices")]
        public List<SalePrice>? SalePrices { get; set; }

        [JsonProperty("buyPrice")]
        public BuyPrice? BuyPrice { get; set; }

        [JsonProperty("barcodes")]
        public List<Barcode>? Barcodes { get; set; }

        [JsonProperty("paymentItemType")]
        public string PaymentItemType { get; set; } = string.Empty;

        [JsonProperty("discountProhibited")]
        public bool DiscountProhibited { get; set; }

        [JsonProperty("article")]
        public string Article { get; set; } = string.Empty;

        [JsonProperty("weight")]
        public double Weight { get; set; }

        [JsonProperty("volume")]
        public double Volume { get; set; }

        [JsonProperty("variantsCount")]
        public int VariantsCount { get; set; }

        [JsonProperty("isSerialTrackable")]
        public bool IsSerialTrackable { get; set; }

        [JsonProperty("trackingType")]
        public string TrackingType { get; set; } = string.Empty;

        [JsonProperty("files")]
        public Files? Files { get; set; }

        [JsonProperty("attributes")]
        public Attributes[]? Attributes { get; set; }
    }

    public class Attributes
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class ProductRoot
    {
        [JsonProperty("context")]
        public Context? Context { get; set; }

        [JsonProperty("meta")]
        public Meta? Meta { get; set; }

        [JsonProperty("rows")]
        public List<Product>? Rows { get; set; }
    }
}
