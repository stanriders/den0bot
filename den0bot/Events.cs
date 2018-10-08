// den0bot (c) StanR 2018 - MIT License
using den0bot.Util;

namespace den0bot
{
    public static class Events
    {
        public static string Event()
        {
            switch (RNG.NextNoMemory(0, 1000))
            {
                case 9: return "Егорыч, бросай наркотики";
                case 99: return "Панда - щитпостер";
                case 999: return "Не хочу";
                case 8: return "Жаба, поебемся после работы?";
                case 88: return "Панда - объебос";
                case 888: return "Ноборееее";
                case 7: return "Шлад, когда 900пп?";
                case 77: return "ккк - самая лучшая девочка!";
                case 777: return "Не получилось чет";
                default: return string.Empty;
            }
        }
        public static string Annoy()
        {
            switch (RNG.NextNoMemory(0, 9))
            {
                case 0: return "Чот лень";
                case 1: return "Панда - щитпостер";
                case 2: return "Не хочу";
                case 3: return "Отстань";
                case 4: return "Панда - главный щитпостер";
                case 5: return "Ноборееее";
                case 6: return "Ни";
                case 7: return "Точно?";
                case 8: return "А сам не можешь?";
                default: return string.Empty;
            }
        }
        public static string RatingRepeat()
        {
            switch (RNG.NextNoMemory(0, 9))
            {
                case 0: return "Держи в курсе";
                case 1: return "Я с первого раза понял";
                case 2: return "Не хочу";
                case 3: return "было)";
                case 4: return "А тудути - евген";
                case 5: return "Ты че шиз";
                case 6: return "Ни";
                case 7: return "заяц_адидас.вебп";
                case 8: return "Ты уже голосовал";
                default: return string.Empty;
            }
        }
    }
}
