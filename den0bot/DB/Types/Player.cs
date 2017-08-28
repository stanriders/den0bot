using SQLite;

namespace den0bot.DB.Types
{
    class Player
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FriendlyName { get; set; }

        public uint OsuID { get; set; }

        public string Topscores { get; set; }

        public long ChatID { get; set; }
    }
}
