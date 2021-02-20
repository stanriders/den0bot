den0bot - osu!-related telegram chat bot
==============
[![Build status](https://ci.appveyor.com/api/projects/status/113qhc1dsm4q5c3p?svg=true)](https://ci.appveyor.com/project/stanriders/den0bot) [![CodeFactor](https://www.codefactor.io/repository/github/stanriders/den0bot/badge)](https://www.codefactor.io/repository/github/stanriders/den0bot)

Uses [Telegram.Bot](https://github.com/TelegramBots/telegram.bot), [SQLite-net](https://github.com/praeclarum/sqlite-net), [Newtonsoft.Json](https://www.newtonsoft.com/json), [OppaiSharp](https://github.com/stanriders/OppaiSharp), [xFFmpeg.NET](https://github.com/cmxl/FFmpeg.NET), [Highcharts](https://highcharts.com/).

# Configuring
Empty config will generate near the app with the name `config.json`.
```
{
  /* Requiured */
  /* list of modules to use */
  "Modules": [
    "ModBasicCommands"
  ],

  /* Telegram bot token, can be obtained on https://telegram.me/botfather */
  "TelegamToken": "123:abc",

  /* Bot owner username */
  "OwnerUsername": "StanRiders",

  /* Optional */
  /* enables sending random strings instead of commands sometimes (strings can be set in locale file) */
  "UseEvents": false,

  /* Cat API token, can be obtained on https://thecatapi.com/signup */
  "CatToken": ""
}
```

# Installing modules
Module files must be placed into `./Modules` folder, module config will generate in the same folder with `modulename.json`.

# Built-in Modules
 * ModAnalytics: Saves various message data into the sqlite database.
 * ModBasicCommands: Self-explanatory.
 * ModCat: Sends random cat image when it finds "cat" in a message.
 * ModGirls: Stores every picture sent with a tag, sends random pictures from DB with voting buttons.
 * ModRandom: Various rng-based commands.
 * ModSanta: Secret santa handling.
 * ModShmalala: Generates messages using Markov chain.
 * ModThread: Returns thread link and messages from 2ch.hk.
  
---

# Modules.Osu
 * ModBeatmap: Finds osu.ppy.sh/b/ID in messages and returns map info with PP values.
 * ModMaplist: Sends random map from a google spreadsheet.
 * ModMatchFollow: Sends updates about ongoing multiplayer match.
 * ModProfile: Finds osu.ppy.sh/u/ID in messages and returns player info and topscores.
 * ModRecentScores: Returns player's recent/map scores with PP info.
 * ModYoutube: Checks youtube channel for new videos and sends them to every chat that enabled subscription.

 ## Configuring
Empty config will generate in the `Modules` directory with the name `osuconfig.json`.
```
{
  /* Requiured */
  /* osu!API v1 token, can be obtained on https://osu.ppy.sh/p/api */
  "osuToken": "",

  /* osu!API v2 client ID, can be obtained on https://osu.ppy.sh/home/account/edit */
  "osuClientId": "",

  /* osu!API v2 client secret, can be obtained on https://osu.ppy.sh/home/account/edit */
  "osuClientSecret": "",

  /* Google API token, can be obtained on https://console.developers.google.com/apis/credentials */
  "GoogleAPIToken": "",

  /* Youtube channel for chat subscriptions */
  "YoutubeChannelId": null,
}
```

 
---
Visual Studio 2019, .NET 5.0