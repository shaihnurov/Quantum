namespace Server.Model
{
    public class ChatModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public required List<UserChat> Users { get; set; }
        public required List<MessageModel> Messages { get; set; }
    }
}