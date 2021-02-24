// den0bot (c) StanR 2021 - MIT License
using den0bot.Analytics.Data.Types;
using Microsoft.EntityFrameworkCore;

namespace den0bot.Analytics.Data
{
	public sealed class AnalyticsDatabase : DbContext
	{
#if DEBUG
		private const string connection_string = "Filename=./analytics.db";
#else
		private const string connection_string = "Filename=/root/den0bot/analytics.db";
#endif
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
			modelBuilder.Entity<Message>().HasIndex(p => new { p.UserId, p.ChatId });

			modelBuilder.Entity<UserStatsQuery>(eb => { eb.HasNoKey(); });
			modelBuilder.Entity<UserStatsAverageQuery>(eb => { eb.HasNoKey(); });
		}

		public DbSet<UserStatsQuery> UserStatsQuery { get; set; }

		public DbSet<UserStatsAverageQuery> UserStatsAverageQuery { get; set; }

		public DbSet<Message> Messages { get; set; }

		public DbSet<Girl> Girls { get; set; }

		public DbSet<User> Users { get; set; }
	}
}
