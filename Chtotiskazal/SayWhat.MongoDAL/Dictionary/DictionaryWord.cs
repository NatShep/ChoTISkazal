using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.MongoDAL.Dictionary;

public class DictionaryWord {
    // ReSharper disable once InconsistentNaming
    public ObjectId  Id { get; set; }
    [BsonElement(LocalDictionaryRepo.WordFieldBsonName)]
    public string Word { get; set; }
    [BsonElement("ts")]
    public string Transcription { get; set; }
    [BsonElement("l")]
    public Language Language { get; set; }
    [BsonElement("tr")]
    public DictionaryTranslationDbEntity[] Translations { get; set; }
    [BsonElement("src")]
    public TranslationSource Source { get; set; }
    
    public DictionaryTranslationDbEntity GetTranslationOrNull(string ru) => 
        Translations.FirstOrDefault(t => t.Word.Equals(ru, StringComparison.InvariantCultureIgnoreCase));

    public (int foundExamples, int lostExamples) LoadExamples(Dictionary<ObjectId, Example> examples) {

        int foundExamples = 0;
        int lostExamples = 0;
        foreach (var translation in Translations)
        {
            foreach (var dictionaryReferenceToExample in translation.Examples)
            {
                if (dictionaryReferenceToExample.TryLoadExample(examples))
                    foundExamples++;
                else
                    lostExamples++;
            }
        }
        return (foundExamples, lostExamples);
    }
        
}