using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MongoDB.Bson;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;

namespace LearningSetProcedures {

public static class VocabFileTools {
    public static void Save(VocabularyEntry vocab, string path) {
        var options = new JsonSerializerOptions {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };
        File.WriteAllText(path, JsonSerializer.Serialize(vocab, options));
    }

    public static VocabularyEntry Load(string path) {
        return JsonSerializer.Deserialize<VocabularyEntry>(File.ReadAllText(path));
    }

    public static async Task<VocabularyEntry> GetFromMongo(LocalDictionaryService localDictionaryService) {
        var allWords = await localDictionaryService.GetAll();
        var dicWords = new List<DictionaryEntry>();
        foreach (var word in allWords)
        {
            if (word.Language != Language.En)
                continue;
            dicWords.Add(
                new DictionaryEntry {
                    Word = word.Word,
                    Transcription = word.Transcription,
                    Translations = word.Translations.SelectToArray(
                        t => new TranslationEntry() {
                            TranslatedText = t.Word,
                            Examples = t.Examples.Select(
                                            e => new ExampleEntry {
                                                OriginPhrase = e.ExampleOrNull.OriginPhrase,
                                                TranslatedPhrase = e.ExampleOrNull.TranslatedPhrase,
                                            })
                                        .ToList()
                        })
                });
        }

        return new VocabularyEntry { Words = dicWords.ToArray() };
    }

    public static async Task SaveToMongo(LocalDictionaryService localDictionaryService, VocabularyEntry vocabulary) {
        foreach (var vocWord in vocabulary.Words)
        {
            var ws = await localDictionaryService.GetAllTranslationWords(vocWord.Word);
            if (ws != null && ws.Length>0)
            {
                Console.WriteLine($"Skip {vocWord.Word}");
            }
            else
            {
                Console.WriteLine($"Add word {vocWord.Word}...");
                await localDictionaryService.AddNewWord(
                    new SayWhat.MongoDAL.Dictionary.DictionaryWord() {
                        Word = vocWord.Word,
                        Language = Language.En,
                        Source = TranslationSource.Restored,
                        Transcription = vocWord.Transcription,
                        Translations = vocWord.Translations.SelectToArray(
                            f => new DictionaryTranslation {
                                Language = Language.En,
                                Word = f.TranslatedText,
                                Examples = f.Examples.SelectToArray(
                                    e => {
                                        var eid = ObjectId.GenerateNewId();
                                        return new DictionaryReferenceToExample {
                                            ExampleId = eid,
                                            ExampleOrNull = new Example {
                                                Id = eid,
                                                Direction = TranslationDirection.EnRu,
                                                OriginPhrase = e.OriginPhrase,
                                                OriginWord = vocWord.Word,
                                                TranslatedPhrase = e.TranslatedPhrase,
                                                TranslatedWord = f.TranslatedText,
                                            }
                                        };
                                    })
                            })
                    });
            }
        }
    }
}

}