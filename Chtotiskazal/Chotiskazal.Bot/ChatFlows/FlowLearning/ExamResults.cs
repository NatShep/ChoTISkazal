using System.Collections.Generic;
using SayWhat.MongoDAL.Words;

namespace Chotiskazal.Bot.ChatFlows.FlowLearning;

public record ExamResults(
    UserWordModel[] Words,
    Dictionary<string, double> OriginWordsScore,
    int QuestionsPassed,
    int QuestionsCount);