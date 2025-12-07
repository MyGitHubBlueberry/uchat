using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using uchat_server.Models;

namespace uchat_server.Database.Configurations
{
    public class DbUserRelationConfiguration : IEntityTypeConfiguration<DbUserRelation>
    {
        public void Configure(EntityTypeBuilder<DbUserRelation> builder)
        {
            builder.HasKey(ur => ur.Id);

            builder.HasOne(ur => ur.SourceUser)
                .WithMany()
                .HasForeignKey(ur => ur.SourceUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ur => ur.TargetUser)
                .WithMany()
                .HasForeignKey(ur => ur.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(ur => ur.IsBlocked)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ur => ur.IsFriend)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(ur => new { ur.SourceUserId, ur.TargetUserId })
                .IsUnique();
        }
    }
}
