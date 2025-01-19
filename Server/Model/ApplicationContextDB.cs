using Microsoft.EntityFrameworkCore;

namespace Server.Model;

public class ApplicationContextDB : DbContext
{
    public ApplicationContextDB(DbContextOptions<ApplicationContextDB> options) : base(options) { }
    
    public DbSet<UserModel> Users { get; set; }
    public DbSet<ChatModel> Chats { get; set; }
    public DbSet<UserChat> UserChats { get; set; }
    public DbSet<MessageModel> Messages { get; set; }
}