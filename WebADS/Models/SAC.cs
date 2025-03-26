namespace WebADS.Models
{
    public class SAC
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerContact { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public List<string>? Photos { get; set; }
    }
}
