// den0bot (c) StanR 2024 - MIT License

using System.Threading.Tasks;
using Serilog;

namespace den0bot.Modules.Osu.WebAPI
{
	public enum APIVersion
	{
		V1,
		V2
	}

	public abstract class Request<TIn, TOut>
	{
		public abstract APIVersion API { get; }

		public abstract string Address { get; }

		public virtual string? Body { get; }

		public abstract TOut? Process(TIn? data);

		public async Task<TOut?> Execute()
		{
			Log.Debug("Running request {Name} ({Address})", GetType().Name, Address);
			var requestData = await WebApiHandler.MakeApiRequest(this);

			return Process(requestData);
		}
	}
}
