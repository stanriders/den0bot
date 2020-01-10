den0bot - osu!-related telegram chat bot
==============
[![Build status](https://ci.appveyor.com/api/projects/status/113qhc1dsm4q5c3p?svg=true)](https://ci.appveyor.com/project/stanriders/den0bot) [![CodeFactor](https://www.codefactor.io/repository/github/stanriders/den0bot/badge)](https://www.codefactor.io/repository/github/stanriders/den0bot)

Uses [Telegram.Bot](https://github.com/TelegramBots/telegram.bot), [SQLite-net](https://github.com/praeclarum/sqlite-net), [Newtonsoft.Json](https://www.newtonsoft.com/json), [OppaiSharp](https://github.com/stanriders/OppaiSharp), [xFFmpeg.NET](https://github.com/cmxl/FFmpeg.NET), [Highcharts](https://highcharts.com/).

# Modules: 
 * ModAnalytics: Saves message data into a sqlite database.
 * ModBasicCommands: Self-explanatory.
 * ModCat: Sends random cat image when found "cat" in a message.
 * ModGirls: Stores every picture with a tag, sends random picture from DB with voting buttons.
 * ModRandom: Various random-based commands.
 * ModShmalala: Markov chain-based message generator.
 * ModThread: Returns thread link and messages from 2ch.hk.
 * ModYoutube: Checks CirclePeople channel for new videos and sends them to every chat it knows.
  
---

# Modules.Osu:
 * ModBeatmap: Finds osu.ppy.sh/b/ID in messages and returns map info with PP values.
 * ModMaplist: Sends random map from a google spreadsheet.
 * ModMatchFollow: Sends updates about ongoing multiplayer match.
 * ModProfile: Finds osu.ppy.sh/u/ID in messages and returns player info and topscores.
 * ModRecentScores: Returns player's recent/map scores with PP info.

 
---
Visual Studio 2019, .NET Core 3.1