using System;
using SayWhat.MongoDAL.Words;

// ReSharper disable MemberCanBePrivate.Global

namespace SayWhat.Bll
{
    public class QuestionMetric
    {
        public QuestionMetric(UserWordModel pairModel, string examName)
        {
            Word = pairModel.Word;
            Created = DateTime.Now;
            AggregateScoreBefore = pairModel.CurrentScore;
            ExamsPassed = pairModel.QuestionAsked;
            PassedScoreBefore = pairModel.AbsoluteScore;
            PreviousExam = pairModel.LastExam;
            Type = examName;
        }
        
        public string Word { get; set; }
        public DateTime Created { get; set; }
        public DateTime? PreviousExam { get; set; }
        public int ElaspedMs { get; set; }
        public double AggregateScoreBefore { get; set; }
        public int PhrasesCount { get; set; }
        public double PassedScoreBefore { get; set; }
        public int ExamsPassed { get; set; }
        public int Result { get; set; }
        public string Type { get; set; }
    }
}