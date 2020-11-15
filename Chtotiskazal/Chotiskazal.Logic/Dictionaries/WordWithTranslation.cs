using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Dic.Logic.Dictionaries
{
    public class WordPair
    {
        public static WordPair CreateFrom(XdXfWord dto)
        {
            WordPair pair = new WordPair()
            {
                Origin = dto.OriginWord,
                Transcription = dto.Transcription,
            };
            var translations = new HashSet<string>();
            foreach (var translation in dto.Translation.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var filtered = translation.Trim(' ', '\r', '\n');

                if (filtered.StartsWith('~'))
                    continue;
                if (filtered.StartsWith('='))
                    continue;
                if (filtered.StartsWith(pair.Origin))
                    filtered = filtered.Replace(pair.Origin, null).Trim(' ', '\r', '\n', ',');

                if (Regex.IsMatch(filtered, $"^[a-zA-Z]"))
                    continue;

                if (filtered.StartsWith(":"))
                    continue;

                if (string.IsNullOrWhiteSpace(filtered))
                    continue;

                foreach (var word in filtered.Split('~').Select(f => f.Trim(' ', '\r', '\n', ',')))
                {
                    if (!word.Contains('=') && !translations.Contains(word))
                        translations.Add(word);
                }
            }
            pair.Translations = translations.ToArray();
            return pair;
        }
        [JsonPropertyName("Origin")]
        public string Origin { get; set; }
        [JsonPropertyName("Translations")]
        public string[] Translations { get; set; }
        [JsonPropertyName("Transcription")]
        public string Transcription { get; set; }
    }
}