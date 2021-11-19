using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chotiskazal.Bot.Questions;
using MongoDB.Driver;
using SayWhat.Bll.Services;
using SayWhat.MongoDAL.Dictionary;
using SayWhat.MongoDAL.Examples;

namespace Chotiskazal.Investigation {

class Program {
    static void Main() {
        var client = new MongoClient("<key");
        var db = client.GetDatabase("SayWhatDb");
        var collection = db.GetCollection<Qmodel>("questionMetrics");
        var allMetrics = collection.Find(Builders<Qmodel>.Filter.Empty).ToList();

        Console.WriteLine("Count: " + allMetrics.Count);

        Console.WriteLine(GetScoreDistributionReport(allMetrics));

        Console.WriteLine("\r\n# Time metrics");
        Console.WriteLine(GetTimeMetricsReport(allMetrics));

        Console.WriteLine("\r\n# Score metrics");
        Console.WriteLine(GetScoreMetricsReport(allMetrics));

        Console.WriteLine("\r\n# Question metrics with clean questions");
        Console.WriteLine(GetQuestionMetricsReport(allMetrics, false));

        Console.WriteLine("\r\n# Question metrics without clean questions");
        Console.WriteLine(GetQuestionMetricsReport(allMetrics, true));

        Console.WriteLine("\r\n# Question metrics by time");


        Console.WriteLine(GetQuestionMetricsByTimeReport(allMetrics));

        Console.WriteLine("\r\n# Current questions");
        Console.WriteLine(GetCurrentQuestionParametersReport(db));

        //Console.WriteLine(GetTimeIntervalSLices(allMetrics, 200));

        //Console.WriteLine(GetPauseData(allMetrics, 0,5, 100));
        Console.WriteLine("\r\n# Regression report");
        Console.WriteLine(GetRegressionReport(allMetrics));
    }

    private static string GetScoreDistributionReport(List<Qmodel> allMetrics) {
        var scores = allMetrics.GroupBy(a => (int)a.ScoreBefore)
                               .Select(b => new { score = b.Key, count = b.Count() })
                               .OrderBy(b => b.score)
                               .ToList();
        StringBuilder sb = new StringBuilder();
        foreach (var score in scores)
        {
            sb.Append($"{score.score}\t{score.count}\r\n");
        }

        return sb + "\r\n";
    }

    private static string GetScoreMetricsReport(List<Qmodel> allMetrics) {
        var sb = new StringBuilder();

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

    private static string GetTimeMetricsReport(List<Qmodel> allMetrics) {
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

    private static string GetQuestionMetricsByTimeReport(List<Qmodel> allMetrics) {
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
        foreach (var bucket in new[] { sum }.Concat(sorted))
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

    private static string GetTimeIntervalSLices(List<Qmodel> allMetrics, int countInBunch) {
        var intervals = GetTimeSeries(countInBunch, allMetrics);
        StringBuilder sb = new StringBuilder($"BunchSize: {countInBunch}\r\n Intervals ({intervals.Length}) :\r\n");
        foreach (var (interval, _) in intervals)
        {
            var ts = TimeSpan.FromSeconds(interval);
            string res = "";
            if (ts.TotalMinutes < 1)
                res = interval + "s";
            else if (ts.TotalHours < 1)
                res = ts.TotalMinutes.ToString("F1") + "m";
            else if (ts.TotalDays < 1)
                res = (int)ts.TotalHours + "h";
            else res = ts.TotalDays.ToString("F1") + "d";
            sb.Append($"{res}, ");
        }

        return sb.ToString();
    }

    private static string GetPauseData(List<Qmodel> allMetrics, int minScore, int maxScore, int countInBunch) =>
        GetPauseData(
            allMetrics.Where(a => a.ScoreBefore >= minScore && a.ScoreBefore <= maxScore)
                      .Where(a => a.ExamName.EndsWith("Eng Choose"))
                      .ToList(), countInBunch);

    private static string GetPauseData(List<Qmodel> allMetrics, int countInBunch) {
        var intervals = GetTimeSeries(countInBunch, allMetrics);
        var sb = new StringBuilder($"BunchSize: {countInBunch}\r\n Intervals ({intervals.Length}) :\r\n");
        foreach (var (interval, percent) in intervals)
            sb.AppendLine($"{interval}\t{percent:F2}");

        return sb.ToString();
    }

    private static string GetRegressionReport(List<Qmodel> allMetrics) {
        var sb = new StringBuilder("Regression report: \r\n");
        Regress(Math.E);
        return sb.ToString();

        void Regress(double logBase) {
            sb.AppendLine(Measure(0, 5, 100));
            sb.AppendLine(Measure(0, 5, 200));
            sb.AppendLine(MeasureAuto(0, 5));
            sb.AppendLine(MeasureAuto(1, 5));
            sb.AppendLine(MeasureAuto(1, 6));
            sb.AppendLine(MeasureAuto(2, 6));
            sb.AppendLine(MeasureAuto(2, 7));
            sb.AppendLine(MeasureAuto(2, 8));

            sb.AppendLine(MeasureAuto(7, 10));
            sb.AppendLine(MeasureAuto(0, 10));
            sb.AppendLine(MeasureAuto(0, 20));

            
            sb.AppendLine("MOST INTERESTING IS " + MeasureAuto(2, 6));

            var names = allMetrics.GroupBy(g => g.GetNonCleanName()).Select(g => g.Key).ToArray();

            foreach (string name in names)
            {
                sb.AppendLine(MeasureAuto(0, 4, name));
                sb.AppendLine(MeasureAuto(1, 5, name));
                sb.AppendLine(MeasureAuto(2, 6, name));    
                sb.AppendLine(MeasureAuto(3, 7, name));
                sb.AppendLine(MeasureAuto(4, 8, name));    
                sb.AppendLine(MeasureAuto(0, 10, name));    
                sb.AppendLine(MeasureAuto(11, 20, name));
                sb.AppendLine(MeasureAuto(0, 20, name));
            }

            
            string MeasureAuto(int minScore, int maxScore, params string[] examNames) {
                var samples = Where(minScore, maxScore);
                if (examNames.Any())
                    samples = samples.Where(s => examNames.Any(e => s.ExamName.Equals(e, StringComparison.InvariantCultureIgnoreCase))).ToList();
                var bunchSize = samples.Count / 15.4;
                if (bunchSize > 300)
                    bunchSize = 300;
                
                return
                    $"{string.Join("|", examNames),-30}\t{MeasureSamples(minScore, maxScore, samples, (int)bunchSize)}";
            }

            string MeasureSamples(int minScore, int maxScore, List<Qmodel> models, int bunchSize) => $"[{minScore}-{maxScore}]\t" +
                                                                         LinearRegressionFor(models, bunchSize,logBase);
            string Measure(int minScore, int maxScore, int bunchSize) => $"[{minScore}-{maxScore}]\t" +
                                                                         LinearRegressionFor(
                                                                             Where(minScore, maxScore), bunchSize,
                                                                             logBase);

            List<Qmodel> Where(int minScore, int maxScore) =>
                allMetrics.Where(a => a.ScoreBefore >= minScore && a.ScoreBefore <= maxScore).ToList();
        }
    }

    private static string LinearRegressionFor(List<Qmodel> allMetrics, int countInBunch, double logBase) {
        var intervals = GetTimeSeries(countInBunch, allMetrics);
        var points = new List<(double, double)>();
        int prevVal = 0;

        foreach (var (t, p) in intervals)
        {
            if (t == int.MaxValue)
                continue;
            var current = Math.Log((t == 0 ? 1 : t + prevVal) / 2.0, logBase);
            if (double.IsNaN(current))
            { }

            prevVal = t;
            points.Add((current, p));
        }

        //var sb = new StringBuilder(
        //    $"Samples:{allMetrics.Count} BunchSize: {countInBunch}  Intervals ({intervals.Length}) :\r\n");
        (double rsquared, double yintercept, double slope) = Helper.LinearRegression(points);
        return
            $"{allMetrics.Count:0000}\t{countInBunch:000}\t{intervals.Length:000}\t{rsquared:F2}\t{yintercept:F2}\t{slope:f2}";
        //sb.AppendLine($"rsquared = {rsquared:F2} yintercept = {yintercept:F2} slope = {slope:F2}");
        //return sb.ToString();
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
                values.Add((qmodel.PreviousExamDeltaInSecs, (100.0 * passed) / count));
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

    private static string GetQuestionMetricsReport(List<Qmodel> allMetrics, bool ignoreClean) {
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