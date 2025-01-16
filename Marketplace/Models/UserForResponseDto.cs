namespace Marketplace.Models
{
    public class UserForResponseDto
    {
        public string? UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ImageUrl {  get; set; } = string.Empty;
    }
}
