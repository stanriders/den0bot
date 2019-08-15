// den0bot (c) StanR 2018 - MIT License
using System;
using System.Numerics;

namespace den0bot.Util
{
	public static class RNG
	{
		private static Random rng = new Random();
		private static int previousNum = 0;
		private static BigInteger previousBigNum = 0;

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

		public static BigInteger NextBigInteger(BigInteger max)
		{
			var result = NextBigIntegerNoMemory(max);
			if (result == previousBigNum && max > 2)
				return NextBigInteger(max);

			previousBigNum = result;
			return result;
		}

		public static BigInteger NextBigIntegerNoMemory(BigInteger max)
		{
			byte[] buffer = max.ToByteArray();
			BigInteger result;

			do
			{
				rng.NextBytes(buffer);
				buffer[buffer.Length - 1] &= 0x7F; // force sign bit to positive
				result = new BigInteger(buffer);
			} while (result >= max || result == 0);

			return result;
		}
	}
}
