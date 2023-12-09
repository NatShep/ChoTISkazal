using System;
using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;
using SayWhat.MongoDAL.Words;

// ReSharper disable MemberCanBePrivate.Global

namespace SayWhat.MongoDAL.QuestionMetrics;

[BsonIgnoreExtraElements]
public class QuestionMetric {
    [BsonIgnore] private readonly Stopwatch _startTime;

    public QuestionMetric(
        UserWordModel pairModel,
        string examName
    ) {
        _startTime = Stopwatch.StartNew();
        Word = pairModel.Word;
        WordCreated = pairModel.Id.CreationTime;
        ScoreBefore = pairModel.AbsoluteScore;
        AgedScoreBefore = pairModel.Score.AgedScore;
        QuestionsAsked = pairModel.QuestionAsked;
        QuestionsPassed = pairModel.QuestionPassed;

        if (pairModel.LastExam.HasValue)
            PreviousExamDeltaInSecs = (int)(DateTime.Now - pairModel.LastExam.Value).TotalSeconds;
        else
            PreviousExamDeltaInSecs = int.MaxValue;
        ExamName = examName;
    }


    public void OnExamFinished(UserWordScore scoreAfter, bool result) {
        _startTime.Stop();
        ScoreChanging = scoreAfter.AbsoluteScore - ScoreBefore;
        SpendTimeMs = (int)_startTime.ElapsedMilliseconds;
        Result = result;
    }

    [BsonElement("e")] public string ExamName { get; }
    [BsonElement("r")] public bool Result { get; private set; }

    [BsonElement("pts")] public int PreviousExamDeltaInSecs { get; }
    [BsonElement("sb")] public double ScoreBefore { get; }
    [BsonElement("asb")] public double AgedScoreBefore { get; }


    [BsonElement("w")] public string Word { get; }
    [BsonElement("ct")] public DateTime WordCreated { get; }

    [BsonElement("dt")] public int SpendTimeMs { get; private set; }
    [BsonElement("ds")] public double ScoreChanging { get; private set; }

    [BsonElement("qa")] public int QuestionsAsked { get; }
    [BsonElement("qp")] public int QuestionsPassed { get; }
}