using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Core.Entities;

namespace TaskManagement.Infrastructure.Data.Configurations;

public class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.HasKey(t => t.PublicId);

        builder.Property(t => t.Title)
           .IsRequired()
           .HasMaxLength(200);

        builder.Property(t => t.Message)
            .HasMaxLength(1000);
    }
}