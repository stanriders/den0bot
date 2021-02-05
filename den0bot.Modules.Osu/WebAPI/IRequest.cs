// den0bot (c) StanR 2021 - MIT License

namespace den0bot.Modules.Osu.WebAPI
{
	public enum APIVersion
	{
		V1,
		V2
	}

	public interface IRequest<In, Out>
	{
		APIVersion API { get; }

		string Address { get; }

		Out Process(In data);
	}
}
