using System;

namespace den0bot.Modules.Osu.Osu.API
{
	public enum APIVersion
	{
		V1,
		V2
	}
	public interface IRequest
	{
		 APIVersion API { get; }

		 string Address { get; }

		 Type ReturnType { get; }

		 bool ShouldReturnSingle { get; }
	}
}
