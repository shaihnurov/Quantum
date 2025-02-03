namespace WebAPI.Models.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Login { get; set; }
        public string? Email { get; set; }
        public byte[]? Image { get; set; }
    }
}
