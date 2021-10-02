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
            var client = new MongoClient("Place bot token here");
            var db = client.GetDatabase("SayWhatDb");
            var collection = db.GetCollection<Qmodel>("questionMetrics");
            var count = collection.CountDocuments(new BsonDocument());
            Console.WriteLine("Count: "+ count);
            Console.WriteLine(GetQuestionMetricsReport(collection));

            Console.WriteLine(GetCurrentQuestionParametersReport(db));
            
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
            var dictionaryRepo = new DictionaryRepo(db);

            var sb2 = new StringBuilder();
            var allQuestions = new QuestionSelector(new DictionaryService(dictionaryRepo, new ExamplesRepo(db)))
                .AllQuestions;
            var maxNameLen = allQuestions.Max(a => a.Question.Name.Length) + 2;

            foreach (var q in allQuestions)
            {
                sb2.Append(
                    $"\r\n{q.Question.Name.PadRight(maxNameLen)}  {q.ExpectedScore}  {q.Question.GetType().Name}");
            }

            var s= sb2.ToString();
            return s;
        }
    }
}