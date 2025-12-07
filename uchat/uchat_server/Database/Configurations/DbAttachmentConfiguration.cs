using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uchat_server.Models;

namespace uchat_server.Database.Configurations
{
    public class DbAttachmentConfiguration : IEntityTypeConfiguration<DbAttachment>
    {
        public void Configure(EntityTypeBuilder<DbAttachment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Url)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasOne(a => a.Message)
                .WithMany(m => m.Attachments)
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.MessageId);
        }
    }
}
