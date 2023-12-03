using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chotiskazal.Bot.Texts;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot {

public static class InlineButtons {
    public const int MaxCallbackDataByteSizeUtf8 = 64;
    public static InlineKeyboardButton Translation(IInterfaceTexts texts) =>
        Translation(texts.TranslateButton);

    public static InlineKeyboardButton Translation(string text) => new InlineKeyboardButton
        { CallbackData = BotCommands.Translate, Text = text };
    public static InlineKeyboardButton LearningSets(string text)=> new InlineKeyboardButton
        { CallbackData = BotCommands.New, Text = text };

    public static InlineKeyboardButton Settings(string text)=> new InlineKeyboardButton
        { CallbackData = BotCommands.Settings, Text = text };
    public static InlineKeyboardButton Exam(IInterfaceTexts texts) =>
        Exam(texts.LearnButton);

    public static InlineKeyboardButton Exam(string text) => new InlineKeyboardButton
        { CallbackData = BotCommands.Learn, Text = text };

    public static InlineKeyboardButton Stats(IInterfaceTexts texts) => new InlineKeyboardButton
        { CallbackData = BotCommands.Stats, Text = texts.StatsButton };

    public static InlineKeyboardButton HowToUse(IInterfaceTexts texts) => new InlineKeyboardButton
        { CallbackData = BotCommands.Help, Text = texts.HelpButton };

    public static InlineKeyboardButton MainMenu(IInterfaceTexts texts) =>
        MainMenu(texts.MainMenuButton);

    public static InlineKeyboardButton MainMenu(string text) => new InlineKeyboardButton
        { CallbackData = BotCommands.Start, Text = text };

    public static InlineKeyboardButton WellLearnedWords(string text) =>
        new InlineKeyboardButton { CallbackData = BotCommands.Words, Text = text };

    public static InlineKeyboardButton[] CreateVariants(IEnumerable<string> variants)
        => variants.Select(
                       (v, i) => new InlineKeyboardButton {
                           CallbackData = i.ToString(),
                           Text = v ?? throw new InvalidDataException("Keyboard text cannot be null")
                       })
                   .ToArray();

}

}