using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.MongoDAL.Words;

public class UserWordTranslation {
    public UserWordTranslation(string word)
    {
            Word = word;
            Examples = new UserWordTranslationReferenceToExample[0];
            
        }
    public UserWordTranslation()
    {
            Examples = new UserWordTranslationReferenceToExample[0];
        }
    [BsonElement("w")]
    public string Word   { get; set; }

    public bool HasTranscription => !string.IsNullOrWhiteSpace(Transcription);
    [BsonElement("ts")]
    public string Transcription   { get; set; }
    [BsonElement("e")]
    public UserWordTranslationReferenceToExample[] Examples { get; set; }

    public IEnumerable<Example> GetDownloadedExamples() =>
        Examples.Select(e => e.ExampleOrNull).Where(e => e != null);

}