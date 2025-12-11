using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uchat_server.Database.Models;

namespace uchat_server.Database.Configurations
{
    public class DbMessageConfiguration : IEntityTypeConfiguration<DbMessage>
    {
        public void Configure(EntityTypeBuilder<DbMessage> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Iv)
                .IsRequired();  //todo add max length

            builder.Property(m => m.CipheredText)
                .IsRequired();  //todo add max length

            builder.Property(m => m.TimeSent)
                .IsRequired();

            builder.Property(m => m.TimeEdited)
                .IsRequired(false);

            builder.Property(m => m.ChatId)
                .IsRequired();

            builder.HasOne(m => m.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => new { m.SenderId, m.ChatId })
                .HasPrincipalKey(cm => new { cm.UserId, cm.ChatId })
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(m => m.Attachments)
                .WithOne(a => a.Message)
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => m.ChatId);
        }
    }
}
