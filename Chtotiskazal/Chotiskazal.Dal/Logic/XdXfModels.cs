using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Dic.Logic.Dictionaries
{
    public static class XdxfReader
    {
        public static xdxf Read(string path)
        {
            var serializer = new XmlSerializer(typeof(xdxf));
            using (var reader = new StreamReader(path))
            {
                return (xdxf)serializer.Deserialize(reader);
            }
        }
    }
    [XmlRoot("xdxf", Namespace = "", IsNullable = false)]
    public class xdxf
    {
        [XmlElement("full_name")]
        public string Name { get; set; }
        [XmlElement("description")]
        public string Description { get; set; }
        [XmlElement("ar")]
        public List<XdXfWord> Words { get; set; }
    }

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
