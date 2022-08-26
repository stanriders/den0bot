// den0bot (c) StanR 2022 - MIT License
using System;
using System.Numerics;

namespace den0bot.Util
{
	public static class RNG
	{
		private static readonly Random rng = Random.Shared;
		private static int previousNum = 0;
		private static BigInteger previousBigNum = 0;

		private const int max_reroll_iterations = 10;

		public static int Next(int min = 0, int max = int.MaxValue)
		{
			if (max <= 3)
				return NextNoMemory(min, max);

			var result = min;
			var iteration = 0;
			while (iteration < max_reroll_iterations)
			{
				result = rng.Next(min, max);
				if (result == previousNum)
				{
					iteration++;
					continue;
				}

				previousNum = result;
				return result;
			}

			return result;
		}

		public static int NextNoMemory(int min = 0, int max = int.MaxValue)
		{
			return rng.Next(min, max);
		}

		public static BigInteger NextBigInteger(BigInteger max, int min = 0)
		{
			if (max <= 3)
				return NextNoMemory(min, (int) max);

			if (max < int.MaxValue)
				return Next(min, (int) max);

			BigInteger result = 1;
			var iteration = 0;
			while (iteration < max_reroll_iterations)
			{
				result = NextBigIntegerNoMemory(max);
				if (result == previousBigNum)
				{
					iteration++;
					continue;
				}

				previousBigNum = result;
				return result;
			}

			return result;
		}

		public static BigInteger NextBigIntegerNoMemory(BigInteger max, int min = 0)
		{
			byte[] buffer = max.ToByteArray();
			BigInteger result;

			do
			{
				rng.NextBytes(buffer);
				buffer[^1] &= 0x7F; // force sign bit to positive
				result = new BigInteger(buffer);
			} while (result >= max || result < min);

			return result;
		}
	}
}
