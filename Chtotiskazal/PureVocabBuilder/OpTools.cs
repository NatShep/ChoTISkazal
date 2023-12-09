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

namespace PureVocabBuilder;

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

   
    public static (string, string[])[] ReadEnru(string path) {
        var lines = File.ReadAllLines(path);
        return lines.Where(c =>!string.IsNullOrWhiteSpace(c))
            .Select(l => {
                var split = l.Split("\t", StringSplitOptions.RemoveEmptyEntries);
                return (split[0], split[1].Split(",").Select(c => c.Trim()).ToArray());
            })
            .ToArray();
    }
    
   
}