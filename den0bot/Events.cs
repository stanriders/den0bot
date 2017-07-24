using System;

namespace den0bot
{
    public static class Events
    {
        public static string Event()
        {
            Random rng = new Random();
            switch (rng.Next(0, 1000))
            {
                case 9: return "Амати, положи нож";
                case 99: return "Панда - щитпостер";
                case 999: return "Не хочу";
                case 8: return "Жаба, поебемся после физики?";
                case 88: return "Панда - главный щитпостер";
                case 888: return "Ноборееее";
                case 7: return "Шлад, когда 700пп?";
                case 77: return "ккк - лучшая девочка!";
                case 777: return "А сам не можешь?";
                default: return string.Empty;
            }
        }
        public static string Annoy()
        {
            Random rng = new Random();
            switch (rng.Next(0, 9))
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
    }
}
