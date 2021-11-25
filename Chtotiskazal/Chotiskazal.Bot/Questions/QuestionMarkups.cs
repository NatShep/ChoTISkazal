using System.Text;

namespace Chotiskazal.Bot.Questions {
public static class QuestionMarkups {
    private static string QuestionHeader = "```\r\n\\-\\-\\-❔\\-\\-\\-\r\n```";
    public static string FreeTemplate(string s) =>$"{QuestionHeader}\r\n{s}";
    public static string TranslateTemplate(string word, string instruction) 
        => $"{QuestionHeader}\r\n" +
           $"*{word}*" +
           $"\r\n" +
           $"\r\n" +
           $"{instruction}";
    
    public static string TranslateTemplate(string word, string instruction, string example) 
        => $"{QuestionHeader}\r\n" +
           $"*{word}*\r\n" +
           $"    _{example}_ \r\n"+
           $"\r\n" +
           $"\r\n" +
           $"{instruction}";
    public static string TranscriptionTemplate(string word, string instruction) 
        => $"{QuestionHeader}\r\n" +
           $"'{word}'" +
           $"\r\n" +
           $"\r\n" +
           $"{instruction}";
    public static string TranslatesAsTemplate(string a, string translatesAs, string b, string instruction) 
        =>$"{QuestionHeader}\r\n"+
        $"*\"{a}\"*\r\n"+
        $"    _{translatesAs}_ \r\n"+
        $"*\"{b}\"*\r\n" +
        $"\r\n"+
        $"{instruction}\r\n";
}
}