namespace Server.Model
{
    public class MessageModel
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
        public int ChatId { get; set; }
        public ChatModel? Chat { get; set; }
        public int UserId { get; set; }
        public UserModel? User { get; set; }
    }
}