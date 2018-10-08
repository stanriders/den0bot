using System;

namespace den0bot.Util
{
    public static class RNG
    {
        private static Random rng = new Random();
        private static int previousNum = 0;

        public static int Next(int max = int.MaxValue) => Next(0, max);
        public static int Next(int min = 0, int max = int.MaxValue)
        {
            int result = rng.Next(min, max);
            if (result == previousNum && max > 2)
                return Next(min, max);

            previousNum = result;
            return result;
        }

        public static int NextNoMemory(int min = 0, int max = int.MaxValue)
        {
            return rng.Next(min, max);
        }
    }
}
