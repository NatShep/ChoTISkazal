using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.Bll.Dto {

public static class EssentialHelper{
    public static EssentialPhrase ToEssentialPhrase(this Example example) 
        => new(example.OriginPhrase, example.TranslatedPhrase);

    public static Example ToExample(this EssentialPhrase phrase, EssentialWord word, EssentialTranslation translation) =>
        new() {
            Direction = TranslationDirection.EnRu,
            Id = ObjectId.GenerateNewId(),
            OriginPhrase = phrase.En,
            OriginWord = word.En,
            TranslatedWord = translation.Ru,
            TranslatedPhrase = phrase.Ru
        };

}
public class EssentialWord {
    public EssentialWord() {
        
    }
    public EssentialWord(string en, string transcription, List<EssentialTranslation> translations) {
        En = en;
        Transcription = transcription;
        Translations = translations;
    }
    public int? Index { get; set; }
    public string En { get; set; }
    public string Transcription { get; set; }
    public List<EssentialTranslation> Translations { get; set; }
    public IEnumerable<EssentialPhrase> AllPhrases => Translations.SelectMany(t => t.Phrases);
    public override string ToString() => $"{En} [{Transcription}]";
}

public class EssentialTranslation {
    public EssentialTranslation(string ru, List<EssentialPhrase> phrases) {
        Ru = ru;
        Phrases = phrases;
    }
    public string Ru { get; }
    public List<EssentialPhrase> Phrases { get; }
    public override string ToString() => Ru;
}

public class EssentialPhrase {
    public EssentialPhrase(string en, string ru) {
        En = en;
        Ru = ru;
    }

    public string En { get; }
    public string Ru { get; }
    public override string ToString() => $"{En} - {Ru}";
    public override int GetHashCode() => En.ToLower().Trim().GetHashCode() ^ Ru.ToLower().Trim().GetHashCode();
    public override bool Equals(object? obj) {
        if (obj is EssentialPhrase p)
            return AreInvariantEquals(p);
        return false;
    }

    private bool AreInvariantEquals(EssentialPhrase phrase) {
        if (Ru.ToLower().Trim() != phrase.Ru.ToLower().Trim())
            return false;   
        if (En.ToLower().Trim() != phrase.En.ToLower().Trim())
            return false;
        return true;
    }
    public bool Fits(string enWord, string ruWord) {
        if (!En.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries).Contains(enWord))
            return false;

        return Ru.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                 .Any(w => w.CheckCloseness(ruWord) != StringsCompareResult.NotEqual);
    }
}

}