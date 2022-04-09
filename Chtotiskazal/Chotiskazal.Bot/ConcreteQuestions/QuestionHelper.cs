using System.Collections.Generic;
using System.Linq;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions {
public static class QuestionHelper {
    public static string[] GetEngVariants(this IEnumerable<UserWordModel> list, string englishWord, int count)
        => list
           .Where(p => p.Word != englishWord)
           .Select(e => e.Word)
           .Shuffle()
           .Take(count)
           .Append(englishWord)
           .Shuffle()
           .ToArray();
    
    public static string[] GetRuVariants(this IEnumerable<UserWordModel> list, UserWordTranslation translation, int count)
        => list
           .SelectMany(e => e.TextTranslations)
           .Where(e=>e != translation.Word)
           .Distinct()
           .Shuffle()
           .Take(count)
           .Append(translation.Word)
           .Shuffle()
           .ToArray();
}
}   