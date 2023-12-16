using System.Collections.Generic;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Examples;
using SayWhat.MongoDAL.Words;

// ReSharper disable MemberCanBePrivate.Global

namespace SayWhat.Bll.Dto;

public class Translation {
    private const int MaxPhraseLengthForSave = 50;

    public Translation(
        string originText,
        string translatedText,
        string originTranscription,
        TranslationDirection translationDirection,
        TranslationSource source, 
        UserWordType wordType) 
    {
        OriginText = originText;
        EnTranscription = originTranscription ?? "";
        TranslatedText = translatedText;
        TranslationDirection = translationDirection;
        Source = source;
        WordType = wordType;
    }
    
    public Translation(
        string originText,
        string translatedText,
        string originTranscription,
        TranslationDirection translationDirection,
        TranslationSource source,
        List<Example> phrases, 
        UserWordType wordType)
        : this(originText, translatedText, originTranscription, translationDirection, source, wordType) => 
        Examples = phrases;

    public bool CanBeSavedToDictionary =>
        OriginText.Length < MaxPhraseLengthForSave && TranslatedText.Length < MaxPhraseLengthForSave;
    public UserWordType WordType { get; set; }
    public string OriginText { get; }
    public string TranslatedText { get; }
    public TranslationDirection TranslationDirection { get; }
    public string EnTranscription { get; }
    public List<Example> Examples { get; } = new();
    public TranslationSource Source { get; }

    public Translation GetEnRu() =>
        TranslationDirection == TranslationDirection.RuEn
            ? GetReversed()
            : this;

    public Translation GetReversed() =>
        new(TranslatedText, OriginText, "",
            TranslationDirection == TranslationDirection.EnRu
                ? TranslationDirection.RuEn
                : TranslationDirection.EnRu, Source, WordType);
}