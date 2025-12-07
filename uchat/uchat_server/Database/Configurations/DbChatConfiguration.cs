using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uchat_server.Models;

namespace uchat_server.Database.Configurations
{
    public class DbChatConfiguration : IEntityTypeConfiguration<DbChat>
    {

        public void Configure(EntityTypeBuilder<DbChat> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.UseTphMappingStrategy();
        }
    }
}
