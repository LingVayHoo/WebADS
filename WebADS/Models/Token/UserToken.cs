using System.ComponentModel.DataAnnotations;

namespace WebADS.Models.Token
{
    public class UserToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = null!; // Идентификатор пользователя

        [Required]
        public string Token { get; set; } = null!;  // Сгенерированный токен

        public DateTime ExpirationDate { get; set; } // Дата истечения срока действия токена
    }
}
