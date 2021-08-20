using System.Collections.Generic;
using System.Collections.Immutable;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot.Core.Services
{
    public partial class Bot
    {
        public static class Constants
        {
            public static ImmutableList<string> Cancells = ImmutableList.CreateRange(new string[1] { "отмена" });
            public static ImmutableList<string> CallSettings = ImmutableList.CreateRange(new string[1] { "/settings" });
            public static ImmutableList<string> Day = ImmutableList.CreateRange(new string[1] { "день" });
            public static ImmutableList<string> Week = ImmutableList.CreateRange(new string[1] { "неделя" });
            public static ImmutableList<string> Month = ImmutableList.CreateRange(new string[1] { "месяц" });
            public static ImmutableList<string> Year = ImmutableList.CreateRange(new string[1] { "год" });

            public static class Keyboards
            {
                public static ReplyKeyboardMarkup settingKeyboard = new ReplyKeyboardMarkup(new List<List<KeyboardButton>> {
                    new List<KeyboardButton>() { "День" },
                    new List<KeyboardButton>(){ "Неделя" },
                    new List<KeyboardButton>(){ "Месяц" },
                    new List<KeyboardButton>(){ "Год" },
                }, true, true);

                public static ReplyKeyboardMarkup yesNoKeyboard = new ReplyKeyboardMarkup(new List<List<KeyboardButton>> {
                    new List<KeyboardButton>() { "Да" },
                    new List<KeyboardButton>(){ "Нет" },
                }, true, true);

                public static ReplyKeyboardMarkup limtKeyboard = new ReplyKeyboardMarkup(new List<List<KeyboardButton>> {
                    new List<KeyboardButton>() { "10" },
                    new List<KeyboardButton>(){ "20" },
                    new List<KeyboardButton>(){ "50" },
                    new List<KeyboardButton>(){ "150" },
                    new List<KeyboardButton>(){ "1000" },
                }, true, true);

                public static ReplyKeyboardMarkup searchingKeyboard = new ReplyKeyboardMarkup(new List<List<KeyboardButton>> {
                    new List<KeyboardButton>() { "Отмена" } }, true, true);
            }
        }
    }
}
