namespace WebADS.Models.Token
{
    public class ExternalToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;  // IdentityUser.StoreHref
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
