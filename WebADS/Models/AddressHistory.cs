namespace WebADS.Models
{
    public class AddressHistory
    {
        public AddressDBModel addressDBModel { get; set; } = new AddressDBModel();
        public string ChangedBy { get; set; } = string.Empty;
    }
}
