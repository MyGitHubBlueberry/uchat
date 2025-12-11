using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uchat_server.Database.Models;

namespace uchat_server.Database.Configurations
{
    public class DbChatConfiguration : IEntityTypeConfiguration<DbChat>
    {

        public void Configure(EntityTypeBuilder<DbChat> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                .HasMaxLength(200);

            builder.Property(c => c.Description)
                .HasMaxLength(1000);

            builder.Property(c => c.ImageUrl)
                .HasMaxLength(500);

            builder.HasOne(c => c.Owner)
                .WithMany()
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            builder.HasMany(c => c.Members)
                .WithOne(cm => cm.Chat)
                .HasForeignKey(cm => cm.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(c => c.IsGroupChat);
        }
    }
}
