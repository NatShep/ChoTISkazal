using System.Collections.Generic;

namespace SayWhat.Bll.Dto;

public class LearningSetDescription {
    public LearningSetDescription(string shortName, string enName, string ruName, string ruDescription,
        string enDescription, List<EssentialWord> words) {
        ShortName = shortName;
        EnName = enName;
        RuName = ruName;
        RuDescription = ruDescription;
        EnDescription = enDescription;
        Words = words;
    }

    public List<EssentialWord> Words { get; }
    public string ShortName { get; }
    public string EnName { get; }
    public string RuName { get; }
    public string EnDescription { get; }
    public string RuDescription { get; }
}