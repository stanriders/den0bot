den0bot - osu!-related telegram chat bot
==============
[![Build status](https://ci.appveyor.com/api/projects/status/113qhc1dsm4q5c3p?svg=true)](https://ci.appveyor.com/project/stanriders/den0bot)

Uses [Telegram.Bot](https://github.com/TelegramBots/telegram.bot), [SQLite-net](https://github.com/praeclarum/sqlite-net), [Newtonsoft.Json](https://www.newtonsoft.com/json), [Meebey.SmartIrc4net](http://www.meebey.net/projects/smartirc4net/)

# Modules: 
 * ModAutohost: Creates a multiplayer lobby and manages host rotation.
 * ModBeatmap: Finds osu.ppy.sh/b/ID in messages and returns map info. Requires [Oppai](https://github.com/Francesco149/oppai) executable.
 * ModCat: Sends random cat image when found "cat" in a message.
 * ModMaplist: Sends random map from a certain google spreadsheet.
 * ModProfile: Finds osu.ppy.sh/u/ID in messages and returns player info and topscores.
 * ModRandom: Various random-based commands.
 * ModRecentScores: Returns recent player's scores with PP info. Requires [Oppai](https://github.com/Francesco149/oppai).
 * ModSettings: Admin commands.
 * ModThread: Returns thread link and messages from 2ch.hk.
 * ModTopscores: Checks topscores of every player listed in Userlist.cs and sends updates to every chat it knows.
 * ModYoutube: Checks osu!content channel for new videos and sends them to every chat it knows.
 
---
Visual Studio 2015