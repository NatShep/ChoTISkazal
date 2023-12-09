﻿using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class EngChoosePhraseQuestion : IQuestion
{
    public bool NeedClearScreen => false;
    public string Name => "Eng Choose Phrase";
    public double PassScore => 0.6;
    public double FailScore => 0.6;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList)
    {
        if (!word.HasAnyExamples)
            return QuestionResult.Impossible;

        var targetPhrase = word.GetRandomExample();

        var other = examList
            .SelectMany(e => e.Examples)
            .Where(p => !p.TranslatedPhrase.AreEqualIgnoreCase(targetPhrase.TranslatedPhrase))
            .Shuffle()
            .Take(5)
            .ToArray();

        if(!other.Any())
            return QuestionResult.Impossible;
            
        var variants = other
            .Append(targetPhrase)
            .Select(e => e.TranslatedPhrase)
            .Shuffle()
            .ToArray();
            
        var choice = await QuestionHelper.ChooseVariantsFlow(chat, targetPhrase.OriginPhrase, variants);
        if(choice==null)
            return QuestionResult.RetryThisQuestion;
            
        return choice.AreEqualIgnoreCase(targetPhrase.TranslatedPhrase)
            ? QuestionResult.Passed(chat.Texts)
            : QuestionResult.Failed(chat.Texts);
    }
}