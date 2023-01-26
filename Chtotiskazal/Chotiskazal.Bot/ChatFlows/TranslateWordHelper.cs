using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.LongDataForTranslationButton;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows {

public static class AddWordHelper {
    static async Task<string> CreateButtonDataFor(CallbackDataForButtonService callbackDataForButtonService, Translation translation,
        bool isSelected) {
        var buttonData = callbackDataForButtonService.CreateButtonDataForShortTranslation(translation, isSelected);
        Console.WriteLine(buttonData + " " + buttonData.Length);
        Console.WriteLine(System.Text.Encoding.UTF8.GetByteCount(buttonData));
        return System.Text.Encoding.UTF8.GetByteCount(buttonData) >= InlineButtons.MaxCallbackDataByteSizeUtf8
            ? await callbackDataForButtonService.CreateButtonDataForLongTranslate(translation, isSelected)
            : buttonData;
    }
    
    public static async Task<InlineKeyboardButton> CreateButtonFor(
        CallbackDataForButtonService callbackDataForButtonService,
        Translation translation,
        bool selected)
        => new InlineKeyboardButton
        {
            CallbackData = await CreateButtonDataFor(callbackDataForButtonService, translation, selected),
            Text = selected
                ? $"{Emojis.Selected} {translation.TranslatedText}"
                : translation.TranslatedText
        };

    public static int FindIndexOf(IReadOnlyList<Translation> translations, string translation) {
        for (int i = 0; i < translations.Count; i++) {
            if (translations[i].TranslatedText.Equals(translation)) {
                return i;
            }
        }

        return -1;
    }
}
}