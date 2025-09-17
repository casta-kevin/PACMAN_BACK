using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entity;

namespace Infrastructure.Persistence.Configurations
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            // Table name
            builder.ToTable("Players");

            // Primary key
            builder.HasKey(e => e.PlayerId);

            // Properties configuration
            builder.Property(e => e.PlayerId)
                .HasColumnName("PlayerId")
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(e => e.Username)
                .HasColumnName("Username")
                .HasColumnType("NVARCHAR(50)")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("DATETIME2")
                .HasDefaultValueSql("SYSDATETIME()")
                .IsRequired();

            // Indexes
            builder.HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("IX_Players_Username");

            // Relationships
            builder.HasMany(e => e.GameSessions)
                .WithOne(e => e.Player)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GameSessions_Players_PlayerId");
        }
    }
}