using Microsoft.EntityFrameworkCore;

namespace Server.Model;

public class ApplicationContextDB : DbContext
{
    public ApplicationContextDB(DbContextOptions<ApplicationContextDB> options) : base(options) { }
    
    public DbSet<UserModel> Users { get; set; }
}