using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Dic.Logic.Dictionaries
{
    public static class Tools
    {
        public static void Convert(string xdxFilePathFrom, string jsonFilePathTo)
        {
            var res = XdxfReader.Read(xdxFilePathFrom);
            //int count = 0;
            //int allcount = 0;
            var dic = new JsonDictionaryDto()
            {
                Words = new List<WordPair>(res.Words.Count)
            };
            foreach (var word in res.Words)
            {
                var pair = WordPair.CreateFrom(word);
                if (!pair.Translations.Any())
                    continue;
                dic.Words.Add(pair);
            }
            //for (int i = 100; i < 10000; i+=20)
            //{

            //    allcount++;
            //    var pair = WordPair.CreateFrom(res.Words[i]);
            //    if(!pair.Translations.Any())
            //        continue;
            //    count++;
            //    Console.WriteLine($"### {pair.Origin} [{pair.Transcription}] [{pair.Translations.Length}]");
            //    foreach (var pairTranslation in pair.Translations)
            //    {
            //        Console.WriteLine("\t"+ pairTranslation);
            //    }
            //    Console.WriteLine();
            //    Console.WriteLine();
            //}
            //Console.WriteLine("ResultCount: "+ count+"/"+ allcount);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            File.WriteAllBytes(jsonFilePathTo, JsonSerializer.SerializeToUtf8Bytes(dic, options));
        }

        public static RuEngDictionary ReadFromFile(string filePath)
        {
            var text = File.ReadAllText(filePath);
            var serialized =  JsonSerializer.Deserialize<JsonDictionaryDto>(text);
            var dic = new RuEngDictionary();
            foreach (var serializedWord in serialized.Words)
            {
                dic.Add(serializedWord.Origin, serializedWord.Transcription, serializedWord.Translations);
            }

            return dic;
        }
    }
}