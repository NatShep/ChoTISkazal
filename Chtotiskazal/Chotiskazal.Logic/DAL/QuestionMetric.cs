using System;

namespace Chotiskazal.Logic.DAL
{
    public class QuestionMetric
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime PreviousExam { get; set; }
        public DateTime WordAdded { get; set; }
        public double AggregateScoreBefore { get; set; }
        public int PhrasesCount { get; set; }
        public double PassedScoreBefore { get; set; }
        public int ExamsPassed { get; set; }
        public int Result { get; set; }
        public string Type { get; set; }
    }
}