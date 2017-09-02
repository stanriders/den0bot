﻿// den0bot (c) StanR 2017 - MIT License
using SQLite;

namespace den0bot.DB.Types
{
    public class Chat
    {
        [PrimaryKey]
        public long Id { get; set; }

        public bool DisableAnnouncements { get; set; }

        public string Admins { get; set; }

        public string Banlist { get; set; }

        //public 
    }
}