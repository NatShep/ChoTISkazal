using System.Collections.Generic;
using System.IO;
using System.Linq;
using Chotiskazal.Bot.InterfaceLang;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot {

public static class InlineButtons {
    public static InlineKeyboardButton Translation(IInterfaceTexts texts) =>
        Translation(texts.TranslateButton);

    public static InlineKeyboardButton Translation(string text) => new InlineKeyboardButton
        { CallbackData = BotCommands.Add, Text = text };

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