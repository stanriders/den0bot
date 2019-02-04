den0bot - osu!-related telegram chat bot
==============
[![Build status](https://ci.appveyor.com/api/projects/status/113qhc1dsm4q5c3p?svg=true)](https://ci.appveyor.com/project/stanriders/den0bot) [![CodeFactor](https://www.codefactor.io/repository/github/stanriders/den0bot/badge)](https://www.codefactor.io/repository/github/stanriders/den0bot)

Uses [Telegram.Bot](https://github.com/TelegramBots/telegram.bot), [SQLite-net](https://github.com/praeclarum/sqlite-net), [Newtonsoft.Json](https://www.newtonsoft.com/json), [Meebey.SmartIrc4net](http://www.meebey.net/projects/smartirc4net/), [OppaiSharp](https://github.com/HoLLy-HaCKeR/OppaiSharp).

# Modules: 
 * ModAutohost: Creates a multiplayer lobby and manages host rotation.
 * ModBasicCommands: Self-explanatory.
 * ModBeatmap: Finds osu.ppy.sh/b/ID in messages and returns map info with PP values.
 * ModCat: Sends random cat image when found "cat" in a message.
 * ModGirls: Stores every picture with a tag, sends random picture from DB with voting buttons.
 * ModMaplist: Sends random map from a google spreadsheet.
 * ModProfile: Finds osu.ppy.sh/u/ID in messages and returns player info and topscores.
 * ModRandom: Various random-based commands.
 * ModRecentScores: Returns recent player's scores with PP info.
 * ModSettings: Admin commands.
 * ModThread: Returns thread link and messages from 2ch.hk.
 * ModTopscores: Checks topscores of every player listed in user DB table and sends updates to every chat it knows.
 * ModYoutube: Checks CirclePeople channel for new videos and sends them to every chat it knows.
 
---
Visual Studio 2017, net461.