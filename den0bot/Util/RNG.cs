// den0bot (c) StanR 2021 - MIT License
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace den0bot.Util
{
	public static class RNG
	{
		private static Random rng = new();
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
				buffer[^1] &= 0x7F; // force sign bit to positive
				result = new BigInteger(buffer);
			} while (result >= max || result == 0);

			return result;
		}

		public static async Task<T> DatabaseRandom<T>(IQueryable<T> query, int amount = -1)
		{
			if (amount == -1)
			{
				amount = await query.CountAsync();
				if (amount <= 0)
					return default;
			}

			int num = Next(max: amount);
			return await query.Skip(num).FirstOrDefaultAsync();
		}
	}
}
