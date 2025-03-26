namespace WebADS.Models
{
    public class StorageSettings
    {
        public Guid Id { get; set; }
        public string StorageID { get; set; } = string.Empty;
        public string StorageName { get; set; } = string.Empty;
        public float OrderPercent { get; set; } = 100;

    }
}
