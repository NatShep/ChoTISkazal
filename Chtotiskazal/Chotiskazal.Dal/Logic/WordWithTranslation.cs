using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Chotiskazal.LogicR
{
    public class WordWithTranslation
    {      
        [JsonPropertyName("Origin")]
        public string Origin { get; set; }
        [JsonPropertyName("Translations")]
        public string[] Translations { get; set; }
        [JsonPropertyName("Transcription")]
        public string Transcription { get; set; }

        [JsonPropertyName("Sourse")]
        public string Sourse { get; set; }

    }
}