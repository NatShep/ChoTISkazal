using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using SayWhat.Bll.Dto;
using SayWhat.Bll.Yapi;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;

namespace SayWhat.Bll {

public static class ChaosBllHelper {
    public static IReadOnlyList<Translation> ToDictionaryTranslations(this DictionaryWord word) =>
        word.Translations.Select(
                t => new Translation(
                    originText: word.Word,
                    translatedText: t.Word,
                    originTranscription: word.Transcription,
                    source: word.Source,
                    translationDirection: word.Language == Language.En
                        ? TranslationDirection.EnRu
                        : TranslationDirection.RuEn,
                    phrases: t.Examples.Select(e => e.ExampleOrNull).ToList()
                ))
            .ToArray();

    public static DictionaryWord ConvertToDictionaryWord(
        string originText, YaDefenition[] definitions,
        Language langFrom,
        Language langTo
    ) {
        if (langFrom == langTo)
            throw new InvalidOperationException();

        var variants = definitions.SelectMany(
            r => r.Tr.Select(
                tr => new {
                    defenition = r,
                    translation = tr,
                }));

        var word = new DictionaryWord {
            Id = ObjectId.GenerateNewId(),
            Language = langFrom,
            Word = originText,
            Source = TranslationSource.Yadic,
            Transcription = definitions.FirstOrDefault()?.Ts,
            Translations = variants.Select(
                                       v => new DictionaryTranslationDbEntity() {
                                           Word = v.translation.Text,
                                           Language = langTo,
                                           Examples = v.translation.Ex?.Select(
                                                           e =>
                                                               new DictionaryReferenceToExample(
                                                                   new Example {
                                                                       Id = ObjectId.GenerateNewId(),
                                                                       OriginWord = originText,
                                                                       TranslatedWord = v.translation.Text,
                                                                       Direction = langFrom == Language.En
                                                                           ? TranslationDirection.EnRu
                                                                           : TranslationDirection.RuEn,
                                                                       OriginPhrase = e.Text,
                                                                       TranslatedPhrase = e.Tr.First().Text,
                                                                   }))
                                                       .ToArray() ??
                                                      Array.Empty<DictionaryReferenceToExample>()
                                       })
                                   .ToArray()
        };
        return word;
    }


    public static string ToJson(this object v) {
        var options = new JsonSerializerOptions {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };
        return JsonSerializer.Serialize(v, options);
    }

    public static void SaveJson(object value, string path) { File.WriteAllText(path, value.ToJson()); }

    public static T LoadJson<T>(string path) {
        var text = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(text);
    }
}

}