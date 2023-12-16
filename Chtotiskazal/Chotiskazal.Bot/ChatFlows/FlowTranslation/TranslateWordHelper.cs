using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SayWhat.Bll.Services;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows.FlowTranslation;

public static class AddWordHelper {
    static async Task<string> CreateButtonDataFor(ButtonCallbackDataService buttonCallbackDataService, SayWhat.Bll.Dto.Translation translation,
        bool isSelected) {
        var buttonData = buttonCallbackDataService.CreateButtonDataForShortTranslation(translation, isSelected);
        Console.WriteLine(buttonData + " " + buttonData.Length);
        Console.WriteLine(System.Text.Encoding.UTF8.GetByteCount(buttonData));
        return System.Text.Encoding.UTF8.GetByteCount(buttonData) >= InlineButtons.MaxCallbackDataByteSizeUtf8
            ? await buttonCallbackDataService.CreateDataForLongTranslation(translation, isSelected)
            : buttonData;
    }
    
    public static async Task<InlineKeyboardButton> CreateButtonFor(
        ButtonCallbackDataService buttonCallbackDataService,
        string text,
        SayWhat.Bll.Dto.Translation translation,
        bool selected)
        => new InlineKeyboardButton
        {
            CallbackData = await CreateButtonDataFor(buttonCallbackDataService, translation, selected),
            Text = selected
                ? $"{Emojis.Selected} {text}"
                : text
        };

    public static int FindIndexOf(IReadOnlyList<SayWhat.Bll.Dto.Translation> translations, string translation) {
        for (int i = 0; i < translations.Count; i++) {
            if (translations[i].TranslatedText.Equals(translation)) {
                return i;
            }
        }

        return -1;
    }
}