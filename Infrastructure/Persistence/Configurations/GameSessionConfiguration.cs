using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entity;

namespace Infrastructure.Persistence.Configurations
{
    public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
    {
        public void Configure(EntityTypeBuilder<GameSession> builder)
        {
            // Table name
            builder.ToTable("GameSessions");

            // Primary key
            builder.HasKey(e => e.GameSessionId);

            // Properties configuration
            builder.Property(e => e.GameSessionId)
                .HasColumnName("GameSessionId")
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(e => e.PlayerId)
                .HasColumnName("PlayerId")
                .IsRequired();

            builder.Property(e => e.Score)
                .HasColumnName("Score")
                .HasColumnType("INT")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(e => e.MaxLevelReached)
                .HasColumnName("MaxLevelReached")
                .HasColumnType("INT")
                .HasDefaultValue(1)
                .IsRequired();

            builder.Property(e => e.PlayedAt)
                .HasColumnName("PlayedAt")
                .HasColumnType("DATETIME2")
                .HasDefaultValueSql("SYSDATETIME()")
                .IsRequired();

            // Indexes
            builder.HasIndex(e => e.PlayerId)
                .HasDatabaseName("IX_GameSessions_PlayerId");

            builder.HasIndex(e => e.Score)
                .HasDatabaseName("IX_GameSessions_Score");

            builder.HasIndex(e => e.PlayedAt)
                .HasDatabaseName("IX_GameSessions_PlayedAt");

            // Relationships
            builder.HasOne(e => e.Player)
                .WithMany(e => e.GameSessions)
                .HasForeignKey(e => e.PlayerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GameSessions_Players_PlayerId");
        }
    }
}