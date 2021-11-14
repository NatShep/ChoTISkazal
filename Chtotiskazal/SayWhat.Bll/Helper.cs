using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SayWhat.Bll {

public static class ChaosHelper {
    public static string ToJson(this object v) {
        var options = new JsonSerializerOptions {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };
        return JsonSerializer.Serialize(v, options);
    }
    public static void SaveJson(object value, string path) {
        File.WriteAllText(path, value.ToJson());
    }
    
    public static T LoadJson<T>(string path) {
        var text  = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(text);
    }
}

}