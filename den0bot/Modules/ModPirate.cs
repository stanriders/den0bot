// den0bot (c) StanR 2017 - MIT License
using System;
using Telegram.Bot.Types;

namespace den0bot.Modules
{
    class ModPirate : IModule
    {
        private readonly double interval = 30.0; // minutes
        private DateTime nextThink;
        private int currentIteration = 0;

        private readonly long receiver = -1001123267177;
        private readonly string dick = 
@"……………………„„-~^^~„-„„_
………………„-^*'' : : „'' : : : : *-„
…………..„-* : : :„„--/ : : : : : : : '\
…………./ : : „-* . .| : : : : : : : : '|
……….../ : „-* . . . | : : : : : : : : |
………...\„-* . . . . .| : : : : : : : :'|
……….../ . . . . . . '| : : : : : : : :|
……..../ . . . . . . . .'\ : : : : : : : |
……../ . . . . . . . . . .\ : : : : : : :|
……./ . . . . . . . . . . . '\ : : : : : /
….../ . . . . . . . . . . . . . *-„„„„-*'
….'/ . . . . . . . . . . . . . . '|
…/ . . . . . . . ./ . . . . . . .| 
../ . . . . . . . .'/ . . . . . . .'|
./ . . . . . . . . / . . . . . . .'|
'/ . . . . . . . . . . . . . . . .'|
'| . . . . . \ . . . . . . . . . .|
'| . . . . . . \„_^- „ . . . . .'|
'| . . . . . . . . .'\ .\ ./ '/ . |
| .\ . . . . . . . . . \ .'' / . '|
| . . . . . . . . . . / .'/ . . .|
| . . . . . . .| . . / ./ ./ . .|";

        public ModPirate()
        {
            Log.Info(this, "Enabled");
            nextThink = DateTime.Now.AddMinutes(15); // so we dont spam too much
        }

        public override string ProcessCommand(string msg, Chat sender)
        {
            if (msg.StartsWith("sendnudes"))
            {
                return dick;
            }
            else if (msg.StartsWith("dudos"))
            {
                API.SendMessage(msg.Substring(6), receiver);
            }
            return string.Empty;
        }

        public override void Think()
        {
            if (nextThink < DateTime.Now)
            {
                Spam();
                nextThink = DateTime.Now.AddMinutes(interval);
            }
        }
        private void Spam()
        {
            if (currentIteration > 3)
                currentIteration = 0;

            switch (currentIteration)
            {
                case 0:
                    {
                        API.SendMessage("Вы используете пробную версию den0bot! Купить подписку: http://kikoe.ru/", receiver);
                        break;
                    }
                case 1:
                    {
                        API.SendPhoto("http://i.imgur.com/jXDkyDl.jpg", receiver);
                        break;
                    }
                case 2:
                    {
                        API.SendMessage(dick, receiver);
                        break;
                    }
                case 3:
                    {
                        API.SendPhoto("http://i.imgur.com/u3lcojt.png", receiver);
                        break;
                    }
                    
            }
            currentIteration++;
        }
    }
}
