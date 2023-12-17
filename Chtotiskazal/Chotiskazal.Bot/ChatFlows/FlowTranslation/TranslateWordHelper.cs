using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Chotiskazal.Bot.Texts;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows.FlowTranslation;

public static class TranslateWordHelper {
    static async Task<string> CreateButtonDataFor(ButtonCallbackDataService buttonCallbackDataService,
        Translation translation, bool isSelected) {
        var buttonData = buttonCallbackDataService.CreateButtonDataForShortTranslation(translation, isSelected);
        Console.WriteLine(buttonData + " " + buttonData.Length);
        Console.WriteLine(Encoding.UTF8.GetByteCount(buttonData));
        return Encoding.UTF8.GetByteCount(buttonData) >= InlineButtons.MaxCallbackDataByteSizeUtf8
            ? await buttonCallbackDataService.CreateDataForLongTranslation(translation, isSelected)
            : buttonData;
    }

    public static async Task<InlineKeyboardButton> CreateButtonFor(ButtonCallbackDataService buttonCallbackDataService,
        string text, Translation translation, bool selected) =>
        new InlineKeyboardButton
        {
            CallbackData = await CreateButtonDataFor(buttonCallbackDataService, translation, selected),
            Text = selected
                ? $"{Emojis.Selected} {text}"
                : text
        };

    public static int FindIndexOf(IReadOnlyList<Translation> translations, string translation) {
        for (int i = 0; i < translations.Count; i++) {
            if (translations[i].TranslatedText.Equals(translation)) {
                return i;
            }
        }

        return -1;
    }

    public static InlineKeyboardButton[] GetTranslateMenuButtons(IInterfaceTexts texts) =>
        new[]
        {
            InlineButtons.MainMenu($"{Emojis.MainMenu} {texts.MainMenuButton}"),
            InlineButtons.Translation($"{texts.ContinueTranslateButton} {Emojis.Translate}")
        };
}