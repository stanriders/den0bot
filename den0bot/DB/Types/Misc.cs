// den0bot (c) StanR 2017 - MIT License
using SQLite;

namespace den0bot.DB.Types
{
    class Misc
    {
        [PrimaryKey]
        public bool Hi { get; set; }

        public int CurrentMPLobby { get; set; }
    }
}
