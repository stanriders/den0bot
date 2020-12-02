// den0bot (c) StanR 2020 - MIT License

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace den0bot.Modules.Osu
{
	public class DatabaseOsu : DbContext
	{
		[Table("Player")]
		public class Player
		{
			[Key]
			public int TelegramID { get; set; }
			public uint OsuID { get; set; }
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite($"Filename={DB.Database.database_path}");
		}

		public DbSet<Player> Players { get; set; }
	}
}
