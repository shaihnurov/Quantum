namespace WebAPI.Models;

public class UserModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public byte[]? Image { get; set; }
}