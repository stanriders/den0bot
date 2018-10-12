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
			["cat_reply"] = "Did someone say {0}?",
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

			["event_1"] = "idke can't ror hr",
			["event_2"] = "ye xD",
			["event_3"] = "727",
			["event_4"] = "idke can ror hr",
			["event_5"] = "Azer isn't so great? Are you kidding me?",
			["event_6"] = "We all know that FunOrange really got 2nd place. He only missed 1, and thats because of the damn star pattern towards the end; pretty much throws everyone off.",
			["event_7"] = "O-oooooooooo AAAAE-A-A-I-A-U- JO-oooooooooooo AAE-O-A-A-U-U-A- E-eee-ee-eee AAAAE-A-E-I-E-A- JO-ooo-oo-oo-oo EEEEO-A-AAA-AAAA",
			["event_8"] = "OwO",
			["event_9"] = "owo",

			["annoy_1"] = "Nah",
			["annoy_2"] = "No",
			["annoy_3"] = "I don't wanna",
			["annoy_4"] = "I'm not in the mood",
			["annoy_5"] = "Cool",
			["annoy_6"] = "Yeah, right",
			["annoy_7"] = "You sure?",
			["annoy_8"] = "Why would I care?",
			["annoy_9"] = "Can't you do it yourself?",

			["rating_repeat_1"] = "You voted already",
			["rating_repeat_2"] = "No",
			["rating_repeat_3"] = "Not again",
			["rating_repeat_4"] = "Please stop",
			["rating_repeat_5"] = "You voted already",
			["rating_repeat_6"] = "No",
			["rating_repeat_7"] = "Not again",
			["rating_repeat_8"] = "Please stop",
			["rating_repeat_9"] = "You can only vote once",

			/*
			["basiccommands_help"] = "Дарова. Короче помимо того, что в списке команд я могу ещё:" + Environment.NewLine + Environment.NewLine +
					"/addplayer <юзернейм> <имя> <osu!айди> - добавить игрока в базу. Бот будет следить за новыми топскорами и сообщать их в чат. Также имя используется в базе щитпостеров." + Environment.NewLine +
					"/removeplayer <имя, указанное при добавлении> - убрать игрока из базы." + Environment.NewLine +
					"/addmeme - добавить мемес базу, можно как ссылку на картинку из интернета, так и загрузить её самому, а команду прописать в подпись." + Environment.NewLine +
					"/disableannouncements - отключить оповещения о новых скорах кукизи." + Environment.NewLine +
					"/enableannouncements - включить их обратно." + Environment.NewLine +
					"/setlocale <en/ru> - сменить язык" + Environment.NewLine +
					"/delet в ответ на девку - удалить девку из базы." + Environment.NewLine + Environment.NewLine +
					"Все эти команды доступны только админам конфы. По вопросам насчет бота писать @StanRiders, но лучше не писать.",
			*/
		};
	}
}
