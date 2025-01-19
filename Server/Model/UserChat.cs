namespace Server.Model
{
    public class UserChat
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required UserModel User { get; set; }
        public int ChatId { get; set; }
        public required ChatModel Chat { get; set; }
    }
}
