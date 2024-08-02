// den0bot (c) StanR 2021 - MIT License
using den0bot.Analytics.Data.Types;
using Microsoft.EntityFrameworkCore;

namespace den0bot.Analytics.Data
{
	public sealed class AnalyticsDatabase : DbContext
	{
		private const string connection_string = "Filename=./data/analytics.db";

		public AnalyticsDatabase()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite(connection_string);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Message>().HasIndex(p => p.ChatId);
			modelBuilder.Entity<Message>().HasIndex(p => new { p.UserId, p.ChatId });

			modelBuilder.Entity<UserStatsQuery>(eb => { eb.HasNoKey(); });
			modelBuilder.Entity<UserStatsAverageQuery>(eb => { eb.HasNoKey(); });
		}

		public DbSet<UserStatsQuery> UserStatsQuery { get; set; } = null!;

		public DbSet<UserStatsAverageQuery> UserStatsAverageQuery { get; set; } = null!;

		public DbSet<Message> Messages { get; set; } = null!;

		public DbSet<Girl> Girls { get; set; } = null!;

		public DbSet<User> Users { get; set; } = null!;
	}
}
