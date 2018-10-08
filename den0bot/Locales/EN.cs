// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;

namespace den0bot.Locales
{
	class EN : ILocale
	{
		public bool Contains(string key) => locale.ContainsKey(key);
		public string GetLocalizedString(string key) => locale[key];
		public string NewMemberGreeting(string name, long userID) => $"Welcome, <a href=\"tg://user?id={userID}\">{name}</a>!";

		private Dictionary<string, string> locale = new Dictionary<string, string>
		{
			["generic_added_to_chat"] = "Sup. You can learn admin commands by sending /help in my PMs." + Environment.NewLine + "Чегоооооо бляяя - /setlocale ru",
			["generic_fail"] = "No can do",

			["beatmap_download"] = "Download",

			["cat_trigger"] = "cat",
			["cat_reply"] = "Did somebody say {0}?",
			["cat_fail"] = "No CATs this time",

			["random_no_shitposter"] = "You're shitposter",
			["random_shitposter"] = " shitposter",
			["random_no_memes"] = "No memes found",
			["random_roll"] = "Rolled ",
			["random_roll_overflow"] = "T-there's no way that could f-fit!!~",

			["recentscores_unknown_player"] = "I don't know you",
			["recentscores_no_scores"] = "No scores",

			["girls_tag"] = "#girls",
			["girls_rating_up"] = "Girl's rating increased ({0})",
			["girls_rating_down"] = "Girl's rating decreased ({0})",
			["girls_rating_delete_lowrating"] = "Girl has been deleted (rating lower -10)",
			["girls_rating_delete_manual"] = "Girl has been deleted",
			["girls_not_found"] = "No girls found",
		};
	}
}
