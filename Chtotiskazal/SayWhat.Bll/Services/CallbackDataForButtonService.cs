using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll.Dto;
using SayWhat.MongoDAL.LongDataForTranslationButton;

namespace SayWhat.Bll.Services {

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

public class CallbackDataForButtonService {
    private readonly LongCallbackDataRepo _longCallbackDataRepository;

    public readonly string Separator = "@";
    public readonly string TranslationDataPrefix = "/trm";
    public readonly string TranslationDataPrefixForLargeSize = "/trl";
    
    public string CreateButtonDataForShortTranslation(Translation translation, bool isSelected)
        => TranslationDataPrefix
           + translation.OriginText
           + Separator
           + translation.TranslatedText + Separator
           + (isSelected ? "1" : "0");

    public async Task<string> CreateButtonDataForLongTranslate(Translation translation, bool isSelected) {
        var data = await GetLongButtonData(translation.TranslatedText);
        if (data is null) {
            data = new LongCallbackData(translation.OriginText, translation.TranslatedText);
            await AddLongButtonData(data);
        }

        return TranslationDataPrefixForLargeSize
               + data.Id
               + Separator
               + (isSelected ? "1" : "0");
    }
    
    public async Task<TranslationButtonData> ParseQueryDataOrNull(string buttonQueryData) {
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
            var translationButtonData = await GetLongButtonData(ObjectId.Parse(translationId));
            if (translationButtonData is null)
                return null;
            return new TranslationButtonData(translationButtonData.Word, translationButtonData.Translation,
                splitted[1] == "1");
        }

        return null;
    }

    public CallbackDataForButtonService(LongCallbackDataRepo repository) {
        _longCallbackDataRepository = repository;
    }
    
    public async Task<LongCallbackData> GetLongButtonData(string translation) {
        return await _longCallbackDataRepository.GetCallbackDataOrDefault(translation);
    }
    
    public async Task<LongCallbackData?> GetLongButtonData(ObjectId translation) {
        return await _longCallbackDataRepository.GetCallbackDataOrDefault(translation);
    }
    
    public async Task AddLongButtonData(LongCallbackData callbackData) {
        await _longCallbackDataRepository.Add(callbackData);
    }
}
}