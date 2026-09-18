using MemoBoard.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MemoBoard.Infrastructure.Data
{
    public class MemoContext : DbContext
    {
        public MemoContext(DbContextOptions<MemoContext> options)
            : base(options) { }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Memo> Memos => Set<Memo>();

        // Two rules the database enforces itself: one account per name,
        // one title per account. Dates are stored and read back in UTC.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var asUtc = new ValueConverter<DateTime, DateTime>(
                toDb => toDb,
                fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc));

            modelBuilder.Entity<Account>(account =>
            {
                account.Property(a => a.UserName).HasMaxLength(30);
                account.HasIndex(a => a.UserName).IsUnique();
                account.Property(a => a.CreatedAt).HasConversion(asUtc);
                account.Property(a => a.LastLoginAt).HasConversion(
                    toDb => toDb,
                    fromDb => fromDb.HasValue ? DateTime.SpecifyKind(fromDb.Value, DateTimeKind.Utc) : null);
                account.HasMany(a => a.Memos).WithOne(m => m.Account).HasForeignKey(m => m.AccountId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Memo>(memo =>
            {
                memo.Property(m => m.Title).HasMaxLength(100);
                memo.Property(m => m.Body).HasMaxLength(2000);
                memo.HasIndex(m => new { m.AccountId, m.Title }).IsUnique();
                memo.Property(m => m.CreatedAt).HasConversion(asUtc);
            });
        }
    }
}
