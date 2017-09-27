// den0bot (c) StanR 2017 - MIT License
using System;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace den0bot
{
    public static class Extensions // todo: remove
    {
        public static string FilterHTML(this string value)
        {
            var step1 = value.Replace("<br>", Environment.NewLine);

            var step2 = Regex.Replace(step1, @"<[^>]+>", "").Trim();

            return Regex.Replace(step2, @"\s{2,}", " ")
                        .Replace("&gt;", ">")
                        .Replace("&nbsp;", " ")
                        .Replace("&quot;", "\"")
                        .Replace("&#47;", "/");
        }

        public static string FilterToHTML(this string value)
        {
            return value.Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;");
        }

        public static string InnerMessageIfAny(this Exception value)
        {
            return value.InnerException?.Message ?? value.Message;
        }

        // this is retarded on so many levels i cant even count them all
        public static Message Clone(this Message value)
        {
            Message result = new Message();
            result.Audio = value.Audio;
            result.AuthorSignature = value.AuthorSignature;
            result.Caption = value.Caption;
            result.ChannelChatCreated = value.ChannelChatCreated;
            result.Chat = value.Chat;
            result.Contact = value.Contact;
            result.Date = value.Date;
            result.DeleteChatPhoto = value.DeleteChatPhoto;
            result.Document = value.Document;
            result.EditDate = value.EditDate;
            result.Entities = value.Entities;
            result.ForwardDate = value.ForwardDate;
            result.ForwardFrom = value.ForwardFrom;
            result.ForwardFromChat = value.ForwardFromChat;
            result.ForwardFromMessageId = value.ForwardFromMessageId;
            result.ForwardSignature = value.ForwardSignature;
            result.From = value.From;
            result.Game = value.Game;
            result.GroupChatCreated = value.GroupChatCreated;
            result.Invoice = value.Invoice;
            result.LeftChatMember = value.LeftChatMember;
            result.Location = value.Location;
            result.MessageId = value.MessageId;
            result.MigrateFromChatId = value.MigrateFromChatId;
            result.MigrateToChatId = value.MigrateToChatId;
            result.NewChatMember = value.NewChatMember;
            result.NewChatMembers = value.NewChatMembers;
            result.NewChatPhoto = value.NewChatPhoto;
            result.NewChatTitle = value.NewChatTitle;
            result.Photo = value.Photo;
            result.PinnedMessage = value.PinnedMessage;
            result.ReplyToMessage = value.ReplyToMessage;
            result.Sticker = value.Sticker;
            result.SuccessfulPayment = value.SuccessfulPayment;
            result.SupergroupChatCreated = value.SupergroupChatCreated;
            result.Text = value.Text;
            result.Venue = value.Venue;
            result.Video = value.Video;
            result.VideoNote = value.VideoNote;
            result.Voice = value.Voice;

            return result;
        }
    }
}
