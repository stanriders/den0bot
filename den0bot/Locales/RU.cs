﻿// den0bot (c) StanR 2018 - MIT License
using System;
using System.Collections.Generic;

namespace den0bot.Locales
{
	class RU : ILocale
	{
		public bool Contains(string key) => locale.ContainsKey(key);
		public string GetLocalizedString(string key) => locale[key];
		public string NewMemberGreeting(string name, long userID) => $"Дороу, <a href=\"tg://user?id={userID}\">{name}</a>" + Environment.NewLine +
																	 "Хорошим тоном является:" + Environment.NewLine +
																	 "<b>1.</b> Кинуть профиль." + Environment.NewLine +
																	 "<b>2.</b> Не инактивить." + Environment.NewLine +
																	 "<b>3.</b> Словить бан при входе." + Environment.NewLine +
																	 "<b>4.</b> Панду бить только ногами, иначе зашкваришься." + Environment.NewLine +
																	 "Ден - аниме, но аниме запрещено. В мульти не играть - мужиков не уважать." + Environment.NewLine +
																	 "<i>inb4 - бан</i>";

		private Dictionary<string, string> locale = new Dictionary<string, string>
		{
			["generic_added_to_chat"] = "Вечер в хату. Админские команды можно узнать с помощью /help мне в личку." + Environment.NewLine + "If you don't understand vodkish - /setlocale en",
			["generic_fail"] = "Чет не получилось",

			["beatmap_download"] = "Скачать",

			["cat_trigger"] = "кот",
			["cat_reply"] = "Кто-то сказал {0}?",
			["cat_fail"] = "Сегодня КОТа не будет...",

			["random_no_shitposter"] = "Ты щитпостер",
			["random_shitposter"] = " - щитпостер",
			["random_no_memes"] = "А мемов-то нет",
			["random_roll"] = "Нароллил ",
			["random_roll_overflow"] = "Нихуя ты загнул",

			["recentscores_unknown_player"] = "ты кто",
			["recentscores_no_scores"] = "Нет скоров",

			["girls_tag"] = "#девки",
			["girls_rating_up"] = "Рейтинг девки повышен ({0})",
			["girls_rating_down"] = "Рейтинг девки понижен ({0})",
			["girls_rating_delete_lowrating"] = "Девка удалена (рейтинг ниже -10)",
			["girls_rating_delete_manual"] = "Девка удалена",
			["girls_not_found"] = "А девок-то нет",

			["basiccommands_help"] = "Дарова. Короче помимо того, что в списке команд я могу ещё:" + Environment.NewLine + Environment.NewLine +
					"/addplayer <юзернейм> <имя> <osu!айди> - добавить игрока в базу. Бот будет следить за новыми топскорами и сообщать их в чат. Также имя используется в базе щитпостеров." + Environment.NewLine +
					"/removeplayer <имя, указанное при добавлении> - убрать игрока из базы." + Environment.NewLine +
					"/addmeme - добавить мемес базу, можно как ссылку на картинку из интернета, так и загрузить её самому, а команду прописать в подпись." + Environment.NewLine +
					"/disableannouncements - отключить оповещения о новых скорах кукизи." + Environment.NewLine +
					"/enableannouncements - включить их обратно." + Environment.NewLine +
					"/setlocale <en/ru> - сменить язык" + Environment.NewLine +
					"/delet в ответ на девку - удалить девку из базы." + Environment.NewLine + Environment.NewLine +
					"Все эти команды доступны только админам конфы. По вопросам насчет бота писать @StanRiders, но лучше не писать.",
		};
	}
}