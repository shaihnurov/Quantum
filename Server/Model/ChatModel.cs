using Server.Model;

public class ChatModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public ICollection<UserChat> Users { get; set; } = [];
    public List<MessageModel> Messages { get; set; } = [];
}