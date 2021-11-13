using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SayWhat.Bll.Dto;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Examples;

namespace PureVocabBuilder {

public static class OpTools {
   
    private static async Task GetAllYapiTranslations() {
        var engrupath = "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/Zip/5000ManualSorted.enru";
        var yapienrupath = "/Users/iurii.sukhanov/Desktop/Features/Buldozerowords/Zip/5000YapiSorted.enru";
        var sortedEngWords = ReadSortedEnglishWordsFromEnru(engrupath);

        var translations = new List<(string, string)>();
        
        var transKey = "<key>";
        var client = new YandexTranslateApiClientIAM(transKey, TimeSpan.FromSeconds(20));
        int i = 0;
        foreach (string engWord in sortedEngWords)
        {
            var translated = await client.Translate(engWord) ?? "---";
            translations.Add((engWord, translated));
            Console.WriteLine($"{i++}: {engWord} {translated}");
        }
        SaveEnru(yapienrupath, translations);

    }
    private static void SaveEnru(string path, List<(string, string)> enru) 
        => File.WriteAllLines(path, enru.Select(e=>e.Item1+"\t"+e.Item2).ToArray());

    public static string[] ReadSortedEnglishWordsFromEnru(string path) {
        var lines = File.ReadAllLines(path);
        return lines.Select(l => l.Split("\t", StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim())
                    .Where(c => c != null)
                    .ToArray();
    }
    
    public static (string, string)[] ReadEnSingleRu(string path) {
        var lines = File.ReadAllLines(path);
        return lines.Where(c =>!string.IsNullOrWhiteSpace(c))
                    .Select(l => {
                        var split = l.Split("\t", StringSplitOptions.RemoveEmptyEntries);
                        return (split[0], split[1]);
                    })
                    .ToArray();
    }

    public static (string en, string ru) Deconstruct(this Example example) {
        var en = example.Direction == TranslationDirection.EnRu ? example.OriginPhrase : example.TranslatedPhrase;
        var ru = example.Direction == TranslationDirection.EnRu ? example.TranslatedPhrase : example.OriginPhrase;
        return (en, ru);
    }
    public static EssentialPhrase DeconstructToPhrase(this Example example) {
        var (en, ru) = example.Deconstruct();
        return new EssentialPhrase(en, ru);
    }
    public static (string, string[])[] ReadEnru(string path) {
        var lines = File.ReadAllLines(path);
        return lines.Where(c =>!string.IsNullOrWhiteSpace(c))
                    .Select(l => {
                        var split = l.Split("\t", StringSplitOptions.RemoveEmptyEntries);
                        return (split[0], split[1].Split(",").Select(c => c.Trim()).ToArray());
                    })
                    .ToArray();
    }
    
    public static void Save<T>(T value, string path) {
        var options = new JsonSerializerOptions {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };
        File.WriteAllText(path, JsonSerializer.Serialize(value, options));
    }
    
    public static T Load<T>(string path) {
        var text  = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(text);
    }
}

}