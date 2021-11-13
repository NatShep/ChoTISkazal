using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SayWhat.MongoDAL;

namespace LearningSetProcedures {

public class LearningSetDescription {
    public static LearningSetDescription ReadFromFile(string path) {
        var lines = File.ReadAllLines(path);
        if (lines.Length < 7)
            throw new ArgumentException();

        if(!lines[0].StartsWith("id: "))
            throw new ArgumentException();
        var id = lines[0].Substring(3).Trim();
        
        if(!lines[1].StartsWith("en: "))
            throw new ArgumentException();
        var enName = lines[1][3..].Trim();
        
        if(!lines[2].StartsWith("ru: "))
            throw new ArgumentException();
        var ruName = lines[2][3..].Trim();
        
        if(!lines[3].StartsWith("end: "))
            throw new ArgumentException();
        var enDesc = lines[3][4..].Trim();
        
        if(!lines[4].StartsWith("rud: "))
            throw new ArgumentException();
        var ruDesc = lines[4][4..].Trim();

        var words = new List<LearningSetWordDescription>();
        foreach (string line in lines.Skip(6))
        {
            if(string.IsNullOrWhiteSpace(line))
                continue;
            var wt = line.Split('\t');
            if (wt.Length != 2)
                throw new ArgumentException();
            var en = wt[0].Trim();
            var ru = wt[1].Split(',').SelectToArray(r => r.Trim());
            var word = new LearningSetWordDescription(en, ru);
            words.Add(word);
        }

        return new LearningSetDescription(id, enName, ruName, enDesc, ruDesc, words.ToArray());
    }
    public LearningSetDescription(string id, string enName, string ruName, string enDescription, string ruDescription, LearningSetWordDescription[] words) {
        Id = id;
        EnName = enName;
        RuName = ruName;
        EnDescription = enDescription;
        RuDescription = ruDescription;
        Words = words;
    }
    public string Id { get; }
    public string EnName { get; }
    public string RuName { get; }
    public string EnDescription { get; }
    public string RuDescription { get; }
    public LearningSetWordDescription[] Words { get; }
    
}

public class LearningSetWordDescription {
    public LearningSetWordDescription(string en, string[] ru) {
        En = en;
        Ru = ru;
    }
    public string En { get; }
    public string[] Ru { get; }
}

}