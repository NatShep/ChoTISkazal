using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.LongDataForTranslationButton;
using Telegram.Bot.Types.ReplyMarkups;

namespace Chotiskazal.Bot.ChatFlows {
public class TranslationButtonData {
    public TranslationButtonData(string origin, string translation, bool isSelected) {
        Origin = origin;
        Translation = translation;
        IsSelected = isSelected;
    }

    public string Origin { get; }
    public string Translation { get; }
    public bool IsSelected { get; }
}

public static class AddWordHelper {
    public const string Separator = "@";
    public const string TranslationDataPrefix = "/trm";
    public const string TranslationDataPrefixForLargeSize = "/trl";

    static async Task<string>
        CreateButtonDataFor(LongDataForButtonService longDataForButtonService, Translation translation,
            bool isSelected) {
        var buttonData = CreateButtonDataForShortTranslation(translation, isSelected);
        Console.WriteLine(buttonData + " " + buttonData.Length);
        Console.WriteLine(System.Text.Encoding.UTF8.GetByteCount(buttonData));
        return System.Text.Encoding.UTF8.GetByteCount(buttonData) >= InlineButtons.MaxCallbackDataByteSizeUtf8
            ? await CreateButtonDataForLongTranslate(longDataForButtonService, translation, isSelected)
            : buttonData;
    }

    static string CreateButtonDataForShortTranslation(Translation translation, bool isSelected)
        => TranslationDataPrefix
           + translation.OriginText
           + Separator
           + translation.TranslatedText + Separator
           + (isSelected ? "1" : "0");

    static async Task<string> CreateButtonDataForLongTranslate(LongDataForButtonService longDataForButtonService,
        Translation translation, bool isSelected) {
        var data = await longDataForButtonService.GetLongButtonData(translation.TranslatedText);
        if (data is null) {
            data = new LongDataForButton(translation.OriginText, translation.TranslatedText);
            await longDataForButtonService.AddLongButtonData(data);
        }

        return TranslationDataPrefixForLargeSize
               + data.Id
               + Separator
               + (isSelected ? "1" : "0");
    }

    public static async Task<TranslationButtonData> ParseQueryDataOrNull(
        LongDataForButtonService longDataForButtonService, string buttonQueryData) {
        if (string.IsNullOrWhiteSpace(buttonQueryData))
            return null;
        if (buttonQueryData.StartsWith(TranslationDataPrefix)) {
            var splitted = buttonQueryData.Substring(4).Split(Separator);
            return splitted.Length != 3 
                ? null 
                : new TranslationButtonData(splitted[0], splitted[1], splitted[2] == "1");
        }

        if (buttonQueryData.StartsWith(TranslationDataPrefixForLargeSize)) {
            var splitted = buttonQueryData.Substring(4).Split(Separator);
            if (splitted.Length != 2)
                return null;
            var translationId = splitted[0];
            var translationButtonData = await longDataForButtonService.GetLongButtonData(ObjectId.Parse(translationId));
            if (translationButtonData is null)
                return null;
            return new TranslationButtonData(translationButtonData.Word, translationButtonData.Translation,
                splitted[1] == "1");
        }

        return null;
    }

    public static async Task<InlineKeyboardButton> CreateButtonFor(
        LongDataForButtonService longDataForButtonService,
        Translation translation,
        bool selected)
        => new InlineKeyboardButton
        {
            CallbackData = await CreateButtonDataFor(longDataForButtonService, translation, selected),
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