using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chotiskazal.Bot.Questions;
using MongoDB.Bson;
using MongoDB.Driver;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;

namespace Chotiskazal.Investigation {

class Program {
    static void Main() {
        var client = new MongoClient("cd");
        var db = client.GetDatabase("SayWhatDb");
        var collection = db.GetCollection<Qmodel>("questionMetrics");
        var count = collection.CountDocuments(new BsonDocument());
        Console.WriteLine("Count: " + count);

        
        Console.WriteLine("\r\n# Time metrics");
         Console.WriteLine(GetTimeMetricsReport(collection));
        
         Console.WriteLine("\r\n# Score metrics");
         Console.WriteLine(GetScoreMetricsReport(collection));
        
         Console.WriteLine("\r\n# Question metrics with clean questions");
         Console.WriteLine(GetQuestionMetricsReport(collection, false));
        
         Console.WriteLine("\r\n# Question metrics without clean questions");
         Console.WriteLine(GetQuestionMetricsReport(collection, true));

        Console.WriteLine("\r\n# Question metrics by time");
        
        
        Console.WriteLine(GetQuestionMetricsByTimeReport(collection));

        Console.WriteLine("\r\n# Current questions");
        Console.WriteLine(GetCurrentQuestionParametersReport(db));
        
        
        Console.WriteLine(GetTimeIntervalSLices(collection, 200));

        Console.WriteLine(GetPauseData(collection, 200));

    }

    private static string GetScoreMetricsReport(IMongoCollection<Qmodel> collection) {
        var sb = new StringBuilder();

        var allMetrics = collection.Find(Builders<Qmodel>.Filter.Empty).ToList();

        sb.AppendLine("Passed: " + allMetrics.Count(a => a.Result));
        sb.AppendLine("Failed: " + allMetrics.Count(a => !a.Result));
        sb.AppendLine(
            $"Total passed: {sb.Append(100 * allMetrics.Count(a => a.Result) / allMetrics.Count) + "%"}");

        var res = allMetrics.GroupBy(b => (int)b.ScoreBefore)
                            .Select(
                                a => new {
                                    score = a.Key,
                                    percent = a.Passed(),
                                    count = a.Count(),
                                })
                            .OrderBy(c => c.score)
                            .ToList();


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

    class TimeBucket {
        public int SecLow;
        public int SecHi;

        public int Count;
        public int Passed;
        public int[] CountByScore = new int[15];
        public int[] PassedByScore = new int[15];

        public void Put(double score, bool result) {
            Count++;

            var item = (int)score;
            if (item >= CountByScore.Length)
                item = CountByScore.Length - 1;

            CountByScore[item]++;
            if (result)
            {
                Passed++;
                PassedByScore[item]++;
            }
        }

        public TimeBucket(int secLow, int secHi) {
            SecLow = secLow;
            SecHi = secHi;
        }

        public override string ToString() {
            if (Count == 0)
                return TimeSpan.FromSeconds(SecLow) + " - " + TimeSpan.FromSeconds(SecHi) + ":   NA";

            var sb = new StringBuilder(
                TimeSpan.FromSeconds(SecLow) +
                " - " +
                TimeSpan.FromSeconds(SecHi) +
                ":   " +
                Passed * 100 / Count +
                " ");

            for (int i = 0; i < CountByScore.Length - 1; i += 2)
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

    private static string GetTimeMetricsReport(IMongoCollection<Qmodel> collection) {
        var buckets = new[] {
            new TimeBucket(0, 10),
            new TimeBucket(10, 60),
            new TimeBucket(60, 6 * 60),
            new TimeBucket(6 * 60, 36 * 60),
            new TimeBucket(36 * 60, 6 * 36 * 60),

            new TimeBucket(6 * 36 * 60, 36 * 36 * 60),
            new TimeBucket(36 * 36 * 60, 6 * 36 * 36 * 60),
            new TimeBucket(6 * 36 * 36 * 60, 36 * 36 * 36 * 60),
            new TimeBucket(36 * 36 * 36 * 60, 6 * 36 * 36 * 36 * 60),
            new TimeBucket(6 * 36 * 36 * 36 * 60, int.MaxValue),
        };


        var allMetrics = collection.Find(Builders<Qmodel>.Filter.Empty).ToList();
        foreach (var metric in allMetrics)
        {
            var buck = buckets.FirstOrDefault(
                b =>
                    b.SecHi > metric.PreviousExamDeltaInSecs && b.SecLow <= metric.PreviousExamDeltaInSecs);
            buck?.Put(metric.ScoreBefore, metric.Result);
        }

        var s = string.Join("\r\n", buckets.Select(s => s.ToString()));
        return s;
    }
        
    private static string GetQuestionMetricsByTimeReport(IMongoCollection<Qmodel> collection) {
        var allMetrics = collection.Find(Builders<Qmodel>.Filter.Empty).ToList();
        var allExamNames = allMetrics.Select(a => a.GetNonCleanName()).ToHashSet();

        var questionToTimeBuckets = new Dictionary<string, QuestionToTimeBucket>();

        foreach (string allExamName in allExamNames)
        {
            questionToTimeBuckets.Add(allExamName, new QuestionToTimeBucket(allExamName));
        }

        foreach (var metric in allMetrics)
        {
            var name = metric.GetNonCleanName();
            questionToTimeBuckets[name].Put(ToCategory7(metric.PreviousExamDeltaInSecs), metric.Result);
        }

        var sorted = questionToTimeBuckets.Values.OrderBy(v => v.Rate).ToArray();
        var sum = sorted.Aggregate((a, b) => a.Sum(b, "Aggregate"));

        var sb = new StringBuilder();
        var maxNameLen = allExamNames.Max(a => a.Length) + 2;
        foreach (var bucket in new[]{sum}.Concat(sorted))
        {
            sb.Append($"\r\n{bucket.QuestionName.PadRight(maxNameLen)}  {(int)bucket.Rate}% |");
            for (int i = 0; i < 10; i++)
            {
                var passed = bucket.GetPercent(i);
                sb.AppendTableItem(passed);
            }
        }

        return sb + "\r\n";

        int ToCategory10(int deltaInSec) {
            if (deltaInSec < 10)
                return 0;
            if (deltaInSec < 60)
                return 1;
            if (deltaInSec < 6 * 60)
                return 2;
            if (deltaInSec < 36 * 60)
                return 3;
            if (deltaInSec < 6 * 36 * 60)
                return 4;
            if (deltaInSec < 36 * 36 * 60)
                return 5;
            if (deltaInSec < 6 * 36 * 36 * 60)
                return 6;
            if (deltaInSec < 36 * 36 * 36 * 60)
                return 7;
            if (deltaInSec < 6 * 36 * 36 * 36 * 60)
                return 8;
            return 9;
        }
        
        int ToCategory7(int deltaInSec) {
            if (deltaInSec < 10)
                return 0;
            if (deltaInSec < 60)
                return 1;
            if (deltaInSec < 36 * 60)
                return 2;
            if (deltaInSec < 36 * 36 * 60)
                return 3;
            if (deltaInSec < 36 * 36 * 36 * 60)
                return 4;
            if (deltaInSec < 6 * 36 * 36 * 36 * 60)
                return 5;
            return 6;
        }
        
        int ToCategory3min(int deltaInSec) {
            if (deltaInSec < 5)
                return 0;
            if (deltaInSec < 10)
                return 1;
            if (deltaInSec < 20)
                return 2;
            if (deltaInSec < 30)
                return 3;
            if (deltaInSec < 60)
                return 4;
            if (deltaInSec < 120)
                return 5;
            return 6;
        }
    }

    private static string GetTimeIntervalSLices(IMongoCollection<Qmodel> collection,int countInBunch) {
        var allMetrics = collection.Find(Builders<Qmodel>.Filter.Empty).ToList();
        var intervals = GetTimeSeries(countInBunch, allMetrics);
        StringBuilder sb = new StringBuilder($"BunchSize: {countInBunch}\r\n Intervals ({intervals.Length}) :\r\n");
        foreach (var (interval, _) in intervals)
        {
            var ts  = TimeSpan.FromSeconds(interval);
            string res = "";
            if (ts.TotalMinutes < 1)
                res = interval + "s";
            else if(ts.TotalHours <1)
                res = ts.TotalMinutes.ToString("F1") + "m";
            else if (ts.TotalDays < 1)
                res = (int)ts.TotalHours + "h";
            else res = ts.TotalDays.ToString("F1") + "d";
            sb.Append($"{res}, ");
        }

        return sb.ToString();
    }
    
    
    private static string GetPauseData(IMongoCollection<Qmodel> collection,int countInBunch) {
        var allMetrics = collection.Find(Builders<Qmodel>.Filter.Empty).ToList();
        var intervals = GetTimeSeries(countInBunch, allMetrics);
        var sb = new StringBuilder($"BunchSize: {countInBunch}\r\n Intervals ({intervals.Length}) :\r\n");
        foreach (var (interval, percent) in intervals) 
            sb.AppendLine($"{interval}\t{percent:F2}");

        return sb.ToString();
    }

    static (int, double)[] GetTimeSeries(int countInBunch, List<Qmodel> models) {
        var orderedByPause = models.OrderBy(m => m.PreviousExamDeltaInSecs).ToList();
        List<(int, double)> values = new List<(int, double)>();
        int count = 0;
        int passed = 0;
        foreach (var qmodel in orderedByPause)
        {
            count++;
            if (qmodel.Result)
                passed++;
            if (count > countInBunch)
            {
                values.Add((qmodel.PreviousExamDeltaInSecs, (100.0*passed)/count));
                count = 0;
                passed = 0;
            }
        }

        return values.ToArray();
    }
    
    class QuestionToTimeBucket {
        public QuestionToTimeBucket(string questionName) { QuestionName = questionName; }

        public string QuestionName { get; }
        public int Count { get; private set; }
        public int Passed { get; private set; }
        public int[] CountByScore = new int[10];
        public int[] PassedByScore = new int[10];

        public int? GetPercent(int timeCategory) {
            if (timeCategory < 0)
                return null;
            if (timeCategory >= CountByScore.Length)
                return null;
            var count = CountByScore[timeCategory];
            if (count < 5)
                return null;
            return 100 * PassedByScore[timeCategory] / count;
        }

        public double Rate => 100.0 * Passed / Count;

        public void Put(int timeBucket, bool result) {
            Count++;

            var item = timeBucket;
            if (item >= CountByScore.Length)
                item = CountByScore.Length - 1;

            CountByScore[item]++;
            if (result)
            {
                Passed++;
                PassedByScore[item]++;
            }
        }

        public QuestionToTimeBucket Sum(QuestionToTimeBucket other, string name) {
            var result = new QuestionToTimeBucket(name) {
                Count = Count + other.Count,
                Passed = Passed + other.Passed,
            };
            for (int i = 0; i < CountByScore.Length; i++)
            {
                result.CountByScore[i] = CountByScore[i] + other.CountByScore[i];
                result.PassedByScore[i] = PassedByScore[i] + other.PassedByScore[i];
            }

            return result;
        }
    }

    private static string GetQuestionMetricsReport(IMongoCollection<Qmodel> collection, bool ignoreClean) {
        var allMetrics = collection.Find(Builders<Qmodel>.Filter.Empty).ToList();
        var allExamNames = allMetrics.Select(a => a.ExamName).ToHashSet();

        var group = ignoreClean
            ? allMetrics.GroupBy(a => a.GetNonCleanName())
            : allMetrics.GroupBy(a => a.ExamName);

        var res = group.Select(
                           a => new {
                               exam = a.Key,
                               percent = a.Passed(),
                               count = a.Count(),
                               scores = a.GroupBy(b => (int)(b.ScoreBefore / 2) * 2)
                                         .Select(
                                             a => new {
                                                 score = a.Key,
                                                 percent = a.Passed(),
                                                 count = a.Count(),
                                             })
                                         .OrderBy(c => c.score)
                                         .ToList()
                           })
                       .OrderBy(r => r.percent)
                       .ToList();
        var maxNameLen = allExamNames.Max(a => a.Length) + 2;

        var sb = new StringBuilder(
            " NAME                              " +
            "passed in count | 0-1 | 2-3 | 4-5 | 6-7 | 8-9 | 10-11 | Оценка" +
            "\r\n----------------------------------------------------------" +
            "-------------------------------");
        foreach (var type in res)
        {
            sb.Append("\r\n" + type.exam.PadRight(maxNameLen) + $" ({type.percent:000}% in {type.count:000}) :");
            for (int i = 0; i < 12; i += 2)
            {
                var a = type.scores.FirstOrDefault(s => s.score == i && s.count > 2);
                sb.AppendTableItem(a?.percent);
            }
        }

        var s = sb.ToString();
        return s;
    }


    private static string GetCurrentQuestionParametersReport(IMongoDatabase db) {
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