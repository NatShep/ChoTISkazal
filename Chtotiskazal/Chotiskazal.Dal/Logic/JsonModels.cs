using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Chotiskazal.LogicR
{
    public class JsonDictionaryDto
    {
        [JsonPropertyName("Words")]
        public List<WordWithTranslation> Words { get; set; }
    }

}
