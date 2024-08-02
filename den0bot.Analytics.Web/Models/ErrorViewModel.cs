// den0bot (c) StanR 2024 - MIT License
namespace den0bot.Analytics.Web.Models
{
	public class ErrorViewModel
	{
		public string RequestId { get; set; } = string.Empty;

		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
	}
}