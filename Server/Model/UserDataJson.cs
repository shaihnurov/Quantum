namespace Server.Model
{
    public class UserDataJson
    {
        public string? Token { get; set; }
        public int? Id { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Login { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}