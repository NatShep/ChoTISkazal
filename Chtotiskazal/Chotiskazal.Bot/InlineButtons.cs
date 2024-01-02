using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chotiskazal.Bot.Texts;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot;

public static class InlineButtons {
    public static InlineKeyboardButton Button(string text, string callbackData) =>
        new(text ?? throw new InvalidDataException("Keyboard text cannot be null")) { CallbackData = callbackData };

    public static InlineKeyboardButton Yes(IInterfaceTexts texts) => Button(texts.YesButton, "1");
    public static InlineKeyboardButton No(IInterfaceTexts texts) => Button(texts.NoButton, "0");

    public static InlineKeyboardButton[] YesNo(IInterfaceTexts texts) => new[] { Yes(texts), No(texts) };

    public const int MaxCallbackDataByteSizeUtf8 = 64;

    public static InlineKeyboardButton Translation(IInterfaceTexts texts) =>
        Translation($"{texts.TranslateButton} {Emojis.Translate}");

    public static InlineKeyboardButton Translation(string text) =>
        Button(text, BotCommands.Translate);

    public static InlineKeyboardButton LearningSets(IInterfaceTexts texts) =>
        Button($"{texts.FrequentWordsButton} {Emojis.LearningSets}", BotCommands.New);

    public static InlineKeyboardButton Settings(IInterfaceTexts texts) =>
        Button($"{texts.SettingsButton} {Emojis.Gear}", BotCommands.Settings);

    public static InlineKeyboardButton Exam(IInterfaceTexts texts) =>
        Exam($"{texts.LearnButton} {Emojis.Learning}");

    public static InlineKeyboardButton Exam(string text) =>
        Button(text, BotCommands.Learn);

    public static InlineKeyboardButton Stats(IInterfaceTexts texts) =>
        Button(texts.StatsButton, BotCommands.Stats);

    public static InlineKeyboardButton HowToUse(IInterfaceTexts texts) =>
        Button(texts.HelpButton, BotCommands.Help);

    public static InlineKeyboardButton MainMenu(IInterfaceTexts texts) =>
        MainMenu($"{Emojis.MainMenu} {texts.MainMenuButton}");

    public static InlineKeyboardButton MainMenu(string text) =>
        Button(text, BotCommands.Start);

    public static InlineKeyboardButton WellLearnedWords(string text) =>
        Button(text, BotCommands.Words);

    public static InlineKeyboardButton[] CreateVariants(IEnumerable<string> variants)
        => variants.Select((v, i) => Button(v, i.ToString())).ToArray();

    public static InlineKeyboardButton Chlang(IInterfaceTexts texts) =>
        Button(texts.ChangeLanguageButton, BotCommands.Chlang);
}