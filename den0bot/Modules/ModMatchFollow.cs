// den0bot (c) StanR 2017 - MIT License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
    class ModMatchFollow : IModule
    {
        private List<uint> followList = new List<uint>();
        private DateTime nextCheck = DateTime.Now;

        public ModMatchFollow()
        {
            AddCommand(new Command()
            {
                Name = "followmatch",
                Action = (msg) => StartFollowing(msg)
            });
        }
        private string StartFollowing(Message msg)
        {
            //followList.Add()
            return "Добавил!";
        }

        public override void Think()
        {
            if (followList.Count > 0 && nextCheck < DateTime.Now)
            {
                Update();
            }
        }

        private /*async*/ void Update()
        {

        }
    }
}
