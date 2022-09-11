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
        var client = new MongoClient("key");
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
        Console.WriteLine(QuestionMetricsReports.GetQuestionMetricsReport(allMetrics, false));

        Console.WriteLine("\r\n# Question metrics without clean questions.");
        Console.WriteLine(QuestionMetricsReports.GetQuestionMetricsReport(allMetrics, true));

        Console.WriteLine("\r\n# Weighted Question metrics without clean questions.");
        Console.WriteLine(QuestionMetricsReports.GetWeightedQuestionMetricsReport(allMetrics));

        Console.WriteLine("\r\n# Question metrics by time");


        Console.WriteLine(QuestionMetricsReports.GetQuestionMetricsByTimeReport(allMetrics));

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
        foreach (var score in scores) {
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


        for (int i = 0; i < 20; i++) {
            var a = res.FirstOrDefault(s => s.score == i);
            if (a == null)
                sb.Append("| --- ");
            else
                sb.Append("| " + a.percent.ToString().PadLeft(3) + " ");
        }

        var s = sb.ToString();
        return s;
    }


    private static string GetTimeMetricsReport(List<Qmodel> allMetrics) {
        var buckets = new[] {
            /*
             *  0   - 20
                10  - 40 *
                20  - 120
                40 - 2*6*20 *
                6*20 - 720
                2*6*20 - 2*720 *
                6*6*20 - 6*720
                2*720 - 12*720  *
                6*720 - 36*720 
                12*720  - 2 * 36 * 720 *
                36*720 - 6 * 36 * 720
                2* 36 * 720 - 12*36*720 *
                6 * 36 * 720 - 36*36*720
             */

            new TimeBucket(0, 10), //
            new TimeBucket(0, 20),
            new TimeBucket(10, 40), //
            new TimeBucket(20, 120),
            new TimeBucket(40, 240), //
            new TimeBucket(120, 720),
            new TimeBucket(240, 1440), //
            new TimeBucket(720, 3 * 1440),
            new TimeBucket(1440, 6 * 1440), //
            new TimeBucket(3 * 1440, 18 * 1440),
            new TimeBucket(6 * 1440, 2 * 18 * 1440), //
            new TimeBucket(18 * 1440, 6 * 18 * 1440),
            new TimeBucket(2 * 18 * 1440, 12 * 18 * 1440), //
            new TimeBucket(4 * 18 * 1440, 6 * 18 * 1440), //
            new TimeBucket(6 * 18 * 1440, 36 * 18 * 1440),
            new TimeBucket(12 * 18 * 1440, 3 * 36 * 18 * 1440), //
            new TimeBucket(36 * 18 * 1440, 6 * 36 * 18 * 1440),
            new TimeBucket(2 * 36 * 18 * 1440, 12 * 36 * 18 * 1440), //

            new TimeBucket(6 * 36 * 18 * 1440, int.MaxValue),

            //new TimeBucket(12*18*1440, int.MaxValue),
        };

        foreach (var metric in allMetrics) {
            var bucks = buckets.Where(
                b =>
                    b.SecHi > metric.PreviousExamDeltaInSecs && b.SecLow <= metric.PreviousExamDeltaInSecs);
            foreach (var buck in bucks) {
                buck.Put(metric.ScoreBefore, metric.Result);
            }
        }

        var s = string.Join("\r\n", buckets.Select(s => s.ToString()));
        return s;
    }


    private static string GetTimeIntervalSLices(List<Qmodel> allMetrics, int countInBunch) {
        var intervals = GetTimeSeries(countInBunch, allMetrics);
        StringBuilder sb = new StringBuilder($"BunchSize: {countInBunch}\r\n Intervals ({intervals.Length}) :\r\n");
        foreach (var (interval, _) in intervals) {
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

            var names = allMetrics.GroupBy(g => Helper.GetNonCleanName(g.ExamName)).Select(g => g.Key).ToArray();

            foreach (string name in names) {
                var normalName = Helper.MapExamName(name);
                sb.AppendLine(MeasureAuto(0, 4, normalName));
                sb.AppendLine(MeasureAuto(1, 5, normalName));
                sb.AppendLine(MeasureAuto(2, 6, normalName));
                sb.AppendLine(MeasureAuto(3, 7, normalName));
                sb.AppendLine(MeasureAuto(4, 8, normalName));
                sb.AppendLine(MeasureAuto(0, 10, normalName));
                sb.AppendLine(MeasureAuto(11, 20, normalName));
                sb.AppendLine(MeasureAuto(0, 20, normalName));
            }


            string MeasureAuto(int minScore, int maxScore, params string[] examNames) {
                var samples = Where(minScore, maxScore);
                if (examNames.Any())
                    samples = samples.Where(s => examNames.Any(e => Helper.MapExamName(s.ExamName).Equals(e, StringComparison.InvariantCultureIgnoreCase))).ToList();
                var bunchSize = samples.Count / 15.4;
                if (bunchSize > 300)
                    bunchSize = 300;

                return
                    $"{string.Join("|", examNames),-30}\t{MeasureSamples(minScore, maxScore, samples, (int)bunchSize)}";
            }

            string MeasureSamples(int minScore, int maxScore, List<Qmodel> models, int bunchSize) => $"[{minScore}-{maxScore}]\t" +
                                                                                                     LinearRegressionFor(models, bunchSize, logBase);
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

        foreach (var (t, p) in intervals) {
            if (t == int.MaxValue)
                continue;
            var current = Math.Log((t == 0 ? 1 : t + prevVal) / 2.0, logBase);
            if (double.IsNaN(current)) { }

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
        foreach (var qmodel in orderedByPause) {
            count++;
            if (qmodel.Result)
                passed++;
            if (count > countInBunch) {
                values.Add((qmodel.PreviousExamDeltaInSecs, (100.0 * passed) / count));
                count = 0;
                passed = 0;
            }
        }

        return values.ToArray();
    }


    private static string GetCurrentQuestionParametersReport(IMongoDatabase db) {
        var dictionaryRepo = new LocalDictionaryRepo(db);

        var sb2 = new StringBuilder();
        var allQuestions = new QuestionSelector(new LocalDictionaryService(dictionaryRepo, new ExamplesRepo(db)))
            .AllQuestions;
        var maxNameLen = allQuestions.Max(a => a.Question.Name.Length) + 2;

        foreach (var q in allQuestions) {
            sb2.Append(
                $"\r\n{q.Question.Name.PadRight(maxNameLen)}  {q.ExpectedScore}  {q.Question.GetType().Name}");
        }

        var s = sb2.ToString();
        return s;
    }
}


}