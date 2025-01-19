namespace Server.Model;

public class UserModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public required string Login { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}