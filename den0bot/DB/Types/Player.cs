// den0bot (c) StanR 2017 - MIT License
using SQLite;

namespace den0bot.DB.Types
{
    public class Player
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FriendlyName { get; set; }

        public uint OsuID { get; set; }

        public string Topscores { get; set; }

        public string Username { get; set; }

        public long ChatID { get; set; }
    }
}
