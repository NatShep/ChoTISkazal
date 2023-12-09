using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll.Dto;
using SayWhat.MongoDAL.LongDataForTranslationButton;

namespace SayWhat.Bll.Services;

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

public class ButtonCallbackDataService {
    private readonly LongCallbackDataRepo _longCallbackDataRepository;

    private const string Separator = "@";
    public const string TranslationDataPrefix = "/trm";
    public const string TranslationDataPrefixForLargeSize = "/trl";
    
    public ButtonCallbackDataService(LongCallbackDataRepo repository) {
        _longCallbackDataRepository = repository;
    }
    
    public string CreateButtonDataForShortTranslation(Translation translation, bool isSelected)
        => TranslationDataPrefix
           + translation.OriginText
           + Separator
           + translation.TranslatedText 
           + Separator
           + (isSelected ? "1" : "0");

    public async Task<string> CreateDataForLongTranslation(Translation translation, bool isSelected) {
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
    
    public async Task<TranslationButtonData> GetButtonDataOrNull(string buttonQueryData) {
        if (string.IsNullOrWhiteSpace(buttonQueryData))
            return null;
        
        // if text in callbackData is less then max size we save translate  to Callbackdata
        if (buttonQueryData.StartsWith(TranslationDataPrefix)) {
            var splitted = buttonQueryData.Substring(4).Split(Separator);
            return splitted.Length != 3
                ? null
                : new TranslationButtonData(splitted[0], splitted[1], splitted[2] == "1");
        }

        // if text in callbackData is more then max size we save translate to BD and write to callbackdata record ID  
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