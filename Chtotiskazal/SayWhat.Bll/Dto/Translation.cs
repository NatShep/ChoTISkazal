using System.Collections.Generic;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Examples;

// ReSharper disable MemberCanBePrivate.Global

namespace SayWhat.Bll.Dto;

public class Translation {
    public Translation(
        string originText,
        string translatedText,
        string originTranscription,
        TranslationDirection translationDirection,
        TranslationSource source) {
        OriginText = originText;
        EnTranscription = originTranscription ?? "";
        TranslatedText = translatedText;
        TranslationDirection = translationDirection;
        Source = source;
    }

    public Translation(
        string originText,
        string translatedText,
        string originTranscription,
        TranslationDirection translationDirection,
        TranslationSource source,
        List<Example> phrases)
        : this(originText, translatedText, originTranscription, translationDirection, source) => Examples = phrases;

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
                : TranslationDirection.EnRu, Source);
}