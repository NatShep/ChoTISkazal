using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Dic.Logic.Dictionaries
{
 
    public class XdXfWord
    {
        [XmlElement("k")]
        public string OriginWord { get; set; }
        [XmlElement("tr")]
        public string Transcription { get; set; }
        [XmlText]
        public string Translation { get; set; }
    }
}
