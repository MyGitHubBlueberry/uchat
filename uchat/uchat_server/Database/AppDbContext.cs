using Microsoft.EntityFrameworkCore;
using uchat_server.Database.Configurations;
using uchat_server.Database.Models;

namespace uchat_server.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DbUser> Users { get; set; }
    public DbSet<DbChat> Chats { get; set; }
    public DbSet<DbMessage> Messages { get; set; }
    public DbSet<DbAttachment> Attachments { get; set; }
    public DbSet<DbUserRelation> UserRelations { get; set; }
    public DbSet<DbChatMember> ChatMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new DbUserConfiguration());
        modelBuilder.ApplyConfiguration(new DbChatConfiguration());
        modelBuilder.ApplyConfiguration(new DbMessageConfiguration());
        modelBuilder.ApplyConfiguration(new DbAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new DbUserRelationConfiguration());
        modelBuilder.ApplyConfiguration(new DbChatMemberConfiguration());
    }
}