namespace Server.Model
{
    public class MessageModel
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public required int ChatId { get; set; }
        public required ChatModel Chat { get; set; }
        public required int UserId { get; set; }
        public required UserModel User { get; set; }
    }
}