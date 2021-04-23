// den0bot (c) StanR 2021 - MIT License

namespace den0bot.Modules.Osu.WebAPI
{
	public enum APIVersion
	{
		V1,
		V2
	}

	public interface IRequest<in TIn, out TOut>
	{
		APIVersion API { get; }

		string Address { get; }

		TOut Process(TIn data);
	}
}
