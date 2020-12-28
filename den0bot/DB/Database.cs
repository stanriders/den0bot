// den0bot (c) StanR 2020 - MIT License

using den0bot.DB.Types;
using Microsoft.EntityFrameworkCore;

namespace den0bot.DB
{
	public sealed class Database : DbContext
	{
		public const string database_path = "./data.db";

		public Database()
		{
			Database.EnsureCreated();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite($"Filename={database_path}");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Misc>().HasNoKey();
		}

		public DbSet<Chat> Chats { get; set; }

		public DbSet<User> Users { get; set; }

		public DbSet<Girl> Girls { get; set; }

		public DbSet<Meme> Memes { get; set; }

		public DbSet<Santa> Santas { get; set; }

		public DbSet<Misc> Misc { get; set; }
	}
}
