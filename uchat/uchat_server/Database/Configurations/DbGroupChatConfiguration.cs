using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uchat_server.Models;

namespace uchat_server.Database.Configurations
{
    public class DbGroupChatConfiguration : IEntityTypeConfiguration<DbGroupChat>
    {
        public void Configure(EntityTypeBuilder<DbGroupChat> builder)
        {
            builder.Property(gc => gc.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(gc => gc.Description)
                .HasMaxLength(1000);

            builder.Property(gc => gc.ImageUrl)
                .HasMaxLength(500);

            builder.HasOne(gc => gc.Owner)
                .WithMany()
                .HasForeignKey(gc => gc.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
