namespace Server.Model.DTO
{
    public class ChatDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<UserDTO>? Users { get; set; }
        public List<MessageDTO>? Messages { get; set; }
    }
}
