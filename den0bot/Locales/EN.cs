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

			/*
			["event_1"] = "Егорыч, бросай наркотики",
			["event_2"] = "Панда - щитпостер",
			["event_3"] = "Не хочу",
			["event_4"] = "Жаба, поебемся после работы?",
			["event_5"] = "Панда - объебос",
			["event_6"] = "Ноборееее",
			["event_7"] = "Шлад, когда 900пп?",
			["event_8"] = "ккк - самая лучшая девочка!",
			["event_9"] = "Не получилось чет",

			["annoy_1"] = "Чот лень",
			["annoy_2"] = "Панда - щитпостер",
			["annoy_3"] = "Не хочу",
			["annoy_4"] = "Отстань",
			["annoy_5"] = "Панда - главный щитпостер",
			["annoy_6"] = "Ноборееее",
			["annoy_7"] = "Ни",
			["annoy_8"] = "Точно?",
			["annoy_9"] = "А сам не можешь?",

			["rating_repeat_1"] = "Держи в курсе",
			["rating_repeat_2"] = "Я с первого раза понял",
			["rating_repeat_3"] = "Не хочу",
			["rating_repeat_4"] = "было)",
			["rating_repeat_5"] = "А тудути - евген",
			["rating_repeat_6"] = "Ты че шиз",
			["rating_repeat_7"] = "Ни",
			["rating_repeat_8"] = "заяц_адидас.вебп",
			["rating_repeat_9"] = "Ты уже голосовал",

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
