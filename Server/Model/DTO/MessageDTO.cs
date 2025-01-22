namespace Server.Model.DTO
{
    public class MessageDTO
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
    }
}
