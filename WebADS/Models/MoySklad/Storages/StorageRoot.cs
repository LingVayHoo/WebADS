using Newtonsoft.Json;

namespace WebADS.Models.MoySklad.Storages
{
    public class RootObject
    {
        [JsonProperty("context")]
        public Context? Context { get; set; }

        [JsonProperty("meta")]
        public Meta? Meta { get; set; }

        [JsonProperty("rows")]
        public List<Row>? Rows { get; set; }
    }

    public class Context
    {
        [JsonProperty("employee")]
        public EmployeeContext? Employee { get; set; }
    }

    public class EmployeeContext
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }
    }

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

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public int? Size { get; set; }

        [JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
        public int? Limit { get; set; }

        [JsonProperty("offset", NullValueHandling = NullValueHandling.Ignore)]
        public int? Offset { get; set; }
    }

    public class Row
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("accountId")]
        public string AccountId { get; set; } = string.Empty;

        [JsonProperty("owner")]
        public Owner? Owner { get; set; }

        [JsonProperty("shared")]
        public bool Shared { get; set; }

        [JsonProperty("group")]
        public Group? Group { get; set; }

        [JsonProperty("updated")]
        public DateTime Updated { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("externalCode")]
        public string ExternalCode { get; set; } = string.Empty;

        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; } = string.Empty;

        [JsonProperty("addressFull")]
        public AddressFull? AddressFull { get; set; }

        [JsonProperty("pathName")]
        public string PathName { get; set; } = string.Empty;

        [JsonProperty("zones")]
        public Zones? Zones { get; set; }

        [JsonProperty("slots")]
        public Slots? Slots { get; set; }
    }

    public class Owner
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }
    }

    public class Group
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }
    }

    public class AddressFull
    {
        [JsonProperty("postalCode")]
        public string PostalCode { get; set; } = string.Empty;

        [JsonProperty("country")]
        public Country? Country { get; set; }

        [JsonProperty("region")]
        public Region? Region { get; set; }

        [JsonProperty("city")]
        public string City { get; set; } = string.Empty;

        [JsonProperty("street")]
        public string Street { get; set; } = string.Empty;

        [JsonProperty("house")]
        public string House { get; set; } = string.Empty;

        [JsonProperty("apartment")]
        public string Apartment { get; set; } = string.Empty;

        [JsonProperty("addInfo")]
        public string AddInfo { get; set; } = string.Empty;

        [JsonProperty("comment")]
        public string Comment { get; set; } = string.Empty;
    }

    public class Country
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }
    }

    public class Region
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }
    }

    public class Zones
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }
    }

    public class Slots
    {
        [JsonProperty("meta")]
        public Meta? Meta { get; set; }
    }
}
