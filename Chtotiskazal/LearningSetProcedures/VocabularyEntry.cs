using System.Collections.Generic;
using SayWhat.MongoDAL;

namespace LearningSetProcedures {

public class VocabularyEntry {
    public DictionaryEntry[] Words { get; set; }
}

public class DictionaryEntry {
    public string Word { get; set; }
    public string Transcription { get; set; }
    public TranslationEntry[] Translations { get; set; }
    public LearningSetEntry LearningSet { get; set; }
}

public class TranslationEntry {
    public string TranslatedText { get; set; }
    public List<ExampleEntry> Examples { get; set; } = new();
}

public class ExampleEntry {
    public string OriginPhrase { get; set; }
    public string TranslatedPhrase { get; set; }
}

public class LearningSetEntry {
    public string From { get; set; }
    public string[] Translations { get; set; }
    public string[] Examples { get; set; }
    public string[] ExampleTranslations { get; set; }
}

}