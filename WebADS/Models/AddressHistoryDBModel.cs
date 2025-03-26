namespace WebADS.Models
{
    public class AddressHistoryDBModel
    {
        public Guid Id { get; set; }
        public string Article { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty; // Create, Update, Delete
        public string OldValues { get; set; } = string.Empty;  // JSON string of old values
        public string NewValues { get; set; } = string.Empty;  // JSON string of new values
        public DateTime ChangeDate { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
    }
}
