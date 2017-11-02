// den0bot (c) StanR 2017 - MIT License
using System.Collections.Generic;
using Telegram.Bot.Types.Enums;

namespace den0bot
{
    abstract class IModule
    {
        public virtual bool NeedsPhotos => false;

        protected List<Command> Commands = new List<Command>();

        protected void AddCommand(Command command)
        {
            Commands.Add(command);
        }

        protected void AddCommands(ICollection<Command> coll)
        {
            Commands.AddRange(coll);
        }

        public Command GetCommand(string name)
        {
            if (Commands.Count <= 0)
                return null;

            name = name.Replace("@den0bot", "");
            return Commands.Find(x => name.StartsWith(x.Name));
        }

        public virtual void Think() {}
    }
}
