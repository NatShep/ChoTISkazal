﻿using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Bot.Questions;
using SayWhat.Bll.Strings;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ConcreteQuestions;

public class RuChooseScenario : IQuestionScenario {
    public QuestionInputType InputType => QuestionInputType.NeedsNoInput;
    public ScenarioWordTypeFit Fit => ScenarioWordTypeFit.OnlyWord;

    public async Task<QuestionResult> Pass(ChatRoom chat, UserWordModel word, UserWordModel[] examList) {
        string[] variants = examList.GetEngVariants(word.Word, 5);

        var ruWord = word.RuTranslations.First().Word;
        var choice = await QuestionScenarioHelper.ChooseVariantsFlow(chat, ruWord, variants);
        if (choice == null)
            return QuestionResult.RetryThisQuestion;

        return choice.AreEqualIgnoreCase(word.Word)
            ? QuestionResult.Passed(chat.Texts)
            : QuestionResult.Failed(chat.Texts);
    }
}