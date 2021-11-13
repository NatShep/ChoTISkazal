using System;
using System.Linq;
using System.Text;
using Chotiskazal.Bot.Questions;
using MongoDB.Bson;
using MongoDB.Driver;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;

namespace Chotiskazal.Investigation
{
    class Program
    {
        static void Main()
        {
            var client = new MongoClient("placeconnectionstringhere");
            var db = client.GetDatabase("SayWhatDb");
            var collection = db.GetCollection<Qmodel>("questionMetrics");
            var count = collection.CountDocuments(new BsonDocument());
            Console.WriteLine("Count: " + count);
            
            Console.WriteLine(GetTimeMetricsReport(collection));

            Console.WriteLine(GetScoreMetricsReport(collection));
            
            Console.WriteLine(GetQuestionMetricsReport(collection));

            Console.WriteLine(GetCurrentQuestionParametersReport(db));
        }

        private static string GetScoreMetricsReport(IMongoCollection<Qmodel> collection)
        {
            var allMetrics = collection.Find(Builders<Qmodel>.Filter.Empty).ToList();
            var res = allMetrics.GroupBy(b => (int) b.ScoreBefore).Select(a => new
            {
                score = a.Key,
                percent = a.Passed(),
                count = a.Count(),
            }).OrderBy(c => c.score).ToList();
            
            var sb = new StringBuilder();
            
            for (int i = 0; i < 20; i++)
            {
                var a = res.FirstOrDefault(s => s.score == i);
                if (a == null)
                    sb.Append("| --- ");
                else
                    sb.Append("| " + a.percent.ToString().PadLeft(3) + " ");
            }
            
            var s = sb.ToString();
            return s;
        }

        class TimeBucket
        {
            public int SecLow;
            public int SecHi;
            
            public int Count;
            public int Passed;
            public int[] CountByScore = new int[15];
            public int[] PassedByScore = new int[15];

            public void Put(double score, bool result)
            {
                Count++;
                
                var item = (int) score;
                if (item >= CountByScore.Length)
                    item = CountByScore.Length-1;

                CountByScore[item]++;
                if (result)
                {
                    Passed++;
                    PassedByScore[item]++;
                }
            }
            public TimeBucket(int secLow, int secHi)
            {
                SecLow = secLow;
                SecHi = secHi;
            }

            public override string ToString()
            {
                if (Count == 0)
                    return TimeSpan.FromSeconds(SecLow)
                           + " - "
                           + TimeSpan.FromSeconds(SecHi)
                           + ":   NA";
                
                var sb = new StringBuilder(
                    TimeSpan.FromSeconds(SecLow)
                    + " - "
                    + TimeSpan.FromSeconds(SecHi)
                    + ":   "
                    + Passed * 100 / Count + " ");

                for (int i = 0; i < CountByScore.Length-1; i+=2)
                {
                    var count = CountByScore[i] + CountByScore[i + 1];
                    var score = PassedByScore[i] + PassedByScore[i + 1];

                    if (count < 3)
                        sb.Append("| --- ");
                    else
                        sb.Append($"| {score * 100 / count:000} ");
                }
                return sb.ToString();
            }
        }

        private static string GetTimeMetricsReport(IMongoCollection<Qmodel> collection)
        {

            var buckets = new[]
            {
                new TimeBucket(0, 10),
                new TimeBucket(10, 60),
                new TimeBucket(60, 6 * 60),
                new TimeBucket(6 * 60, 36 * 60),
                new TimeBucket(36 * 60, 6 * 36 * 60),

                new TimeBucket(6 * 36 * 60, 36 * 36 * 60),
                new TimeBucket(36 * 36 * 60, 6 * 36 * 36 * 60),
                new TimeBucket(6 * 36 * 36 * 60, 36 * 36 * 36 * 60),
                new TimeBucket(36 * 36 * 36 * 60, 6* 36 * 36 * 36 * 60),
                new TimeBucket(6* 36 * 36 * 36 * 60, int.MaxValue),
            };


            var allMetrics = collection.Find(Builders<Qmodel>.Filter.Empty).ToList();
            foreach (var metric in allMetrics)
            {
                var buck = buckets.FirstOrDefault(b =>
                    b.SecHi > metric.PreviousExamDeltaInSecs && b.SecLow <= metric.PreviousExamDeltaInSecs);
                buck?.Put(metric.ScoreBefore,metric.Result);
            }

            var s = string.Join("\r\n", buckets.Select(s=>s.ToString()));
            return s;
        }

        private static string GetQuestionMetricsReport(IMongoCollection<Qmodel> collection)
        {
            var allMetrics = collection.Find(Builders<Qmodel>.Filter.Empty).ToList();
            var allExamNames = allMetrics.Select(a => a.ExamName).ToHashSet();
            var res = allMetrics.GroupBy(a => a.ExamName).Select(a => new
            {
                exam = a.Key,
                percent = a.Passed(),
                count = a.Count(),
                scores = a.GroupBy(b => (int) (b.ScoreBefore / 2) * 2).Select(a => new
                {
                    score = a.Key,
                    percent = a.Passed(),
                    count = a.Count(),
                }).OrderBy(c => c.score).ToList()
            }).OrderBy(r => r.percent).ToList();
            var maxNameLen = allExamNames.Max(a => a.Length) + 2;

            var sb = new StringBuilder(" NAME                              " +
                                       "passed in count | 0-1 | 2-3 | 4-5 | 6-7 | 8-9 | 10-11 | Оценка" +
                                       "\r\n----------------------------------------------------------" +
                                       "-------------------------------");
            foreach (var type in res)
            {
                sb.Append("\r\n" + type.exam.PadRight(maxNameLen) + $" ({type.percent:000}% in {type.count:000}) :");
                for (int i = 0; i < 12; i += 2)
                {
                    var a = type.scores.FirstOrDefault(s => s.score == i && s.count > 2);
                    if (a == null)
                        sb.Append("| --- ");
                    else
                        sb.Append("| " + a.percent.ToString().PadLeft(3) + " ");
                }
            }

            var s = sb.ToString();
            return s;
        }

        private static string GetCurrentQuestionParametersReport(IMongoDatabase db)
        {
            var dictionaryRepo = new LocalDictionaryRepo(db);

            var sb2 = new StringBuilder();
            var allQuestions = new QuestionSelector(new LocalDictionaryService(dictionaryRepo, new ExamplesRepo(db)))
                .AllQuestions;
            var maxNameLen = allQuestions.Max(a => a.Question.Name.Length) + 2;

            foreach (var q in allQuestions)
            {
                sb2.Append(
                    $"\r\n{q.Question.Name.PadRight(maxNameLen)}  {q.ExpectedScore}  {q.Question.GetType().Name}");
            }

            var s = sb2.ToString();
            return s;
        }
    }
}