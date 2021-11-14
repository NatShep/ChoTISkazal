using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Chotiskazal.Investigation
{
    class Qmodel
    {
        public ObjectId Id { get; set; }
        [BsonElement("e")] public string ExamName { get; set; }

        public string GetNonCleanName() => ExamName.Replace("Clean ", "");
        [BsonElement("r")] public bool Result { get; set; }

        [BsonElement("pts")] public int PreviousExamDeltaInSecs { get; set; }
        [BsonElement("sb")] public double ScoreBefore { get; set; }
        [BsonElement("asb")] public double AgedScoreBefore { get; set; }


        [BsonElement("w")] public string Word { get; set; }
        [BsonElement("ct")] public DateTime WordCreated { get; set; }

        [BsonElement("dt")] public int SpendTimeMs { get; set; }
        [BsonElement("ds")] public double ScoreChanging { get; set; }

        [BsonElement("qa")] public int QuestionsAsked { get; }
        [BsonElement("qp")] public int QuestionsPassed { get; }
    }
}