namespace WebADS.Models.Token
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;  // IdentityUser.StoreHref
        public string Token { get; set; } = string.Empty;  // Генерируем случайный GUID
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}
