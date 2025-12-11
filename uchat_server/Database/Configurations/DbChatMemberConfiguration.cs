using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uchat_server.Database.Models;

namespace uchat_server.Database.Configurations
{
    public class DbChatMemberConfiguration : IEntityTypeConfiguration<DbChatMember>
    {
        public void Configure(EntityTypeBuilder<DbChatMember> builder)
        {
            builder.HasKey(cm => new { cm.UserId, cm.ChatId });

            builder.HasOne(cm => cm.User)
                .WithMany(u => u.Chats)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cm => cm.Chat)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cm => cm.LastMessage)
                .WithMany()
                .HasForeignKey(cm => cm.LastMessageId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(cm => cm.IsAdmin)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(cm => cm.IsMuted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(cm => cm.IsBlocked)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(cm => cm.ChatId);
            builder.HasIndex(cm => cm.UserId);
        }
    }
}
