using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chotiskazal.Investigation {
public static class QuestionMetricsReports {
    public static string GetQuestionMetricsByTimeReport(List<Qmodel> allMetrics) {
        var allExamNames = allMetrics.Select(a => Helper.GetNonCleanName(a.ExamName)).ToHashSet();

        var questionToTimeBuckets = new Dictionary<string, QuestionToTimeBucket>();

        foreach (string allExamName in allExamNames) {
            questionToTimeBuckets.Add(allExamName, new QuestionToTimeBucket(allExamName));
        }

        foreach (var metric in allMetrics) {
            var name = Helper.GetNonCleanName(metric.ExamName);
            questionToTimeBuckets[name].Put(ToCategory7(metric.PreviousExamDeltaInSecs), metric.Result);
        }

        var sorted = questionToTimeBuckets.Values.OrderBy(v => v.Rate).ToArray();
        var sum = sorted.Aggregate((a, b) => a.Sum(b, "Aggregate"));

        var sb = new StringBuilder();
        var maxNameLen = allExamNames.Max(a => a.Length) + 2;
        foreach (var bucket in new[] { sum }.Concat(sorted)) {
            sb.Append($"\r\n{bucket.QuestionName.PadRight(maxNameLen)}  {(int)bucket.Rate}% |");
            for (int i = 0; i < 10; i++) {
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

    public static string GetQuestionMetricsReport(List<Qmodel> allMetrics, bool ignoreClean) {
        var allExamNames = allMetrics.Select(a => Helper.GetNonCleanName(a.ExamName)).ToHashSet();

        var group = ignoreClean
            ? allMetrics.GroupBy(a => Helper.GetNonCleanName(a.ExamName))
            : allMetrics.GroupBy(a => Helper.MapExamName(a.ExamName));

        var res = group.Select(
                a => new
                {
                    exam = a.Key,
                    percent = a.Passed(),
                    count = a.Count(),
                    simpleCount = a.Count(c => c.ScoreBefore <= 6),
                    simplePassed = a.Count(c => c.Result && c.ScoreBefore <= 6),
                    scores = a.GroupBy(b => (int)(b.ScoreBefore / 2) * 2)
                        .Select(
                            a => new
                            {
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
        foreach (var type in res) {
            sb.Append("\r\n" + type.exam.PadRight(maxNameLen) + $" ({type.percent:000}% in {type.count:000}) :");
            for (int i = 0; i < 12; i += 2) {
                var a = type.scores.FirstOrDefault(s => s.score == i && s.count > 2);
                sb.AppendTableItem(a?.percent);
            }
        }

        sb.Append("\r\nsimple questions:\r\n");
        foreach (var type in res.OrderBy(c => 100 * c.simplePassed / (double)c.simpleCount)) {
            sb.Append("\r\n" + type.exam.PadRight(maxNameLen) +
                      $" ({100 * type.simplePassed / (double)type.simpleCount:000}% in {type.count:000}) :");
        }

        var s = sb.ToString();
        return s;
    }


    public static string GetQuestionMetricsToTSReport(List<Qmodel> allMetrics) {
        var allExamNames = allMetrics.Select(a => Helper.GetNonCleanName(a.ExamName)).ToHashSet();

        var group = allMetrics.GroupBy(a => Helper.GetNonCleanName(a.ExamName));

        var res = group.Select(
                a => new
                {
                    exam = a.Key,
                    percent = a.Passed(),
                    count = a.Count(),
                    simpleCount = a.Count(c => c.ScoreBefore <= 6),
                    simplePassed = a.Count(c => c.Result && c.ScoreBefore <= 6),
                    hardness = a
                        .GroupBy(x => {
                            var hardness = (int)GetHardness(x);
                            return hardness == int.MaxValue ? 0 : hardness > 100 ? 100 : (hardness / 2) * 2;
                        })
                        .Select(m => new
                        {
                            score = m.Key,
                            count = m.Count(),
                            percent = m.PassedDouble()
                        })
                        .OrderBy(c => c.score)
                        .ToList(),
                })
            .OrderBy(r => r.percent)
            .ToList();
        var maxNameLen = allExamNames.Max(a => a.Length) + 2;

        var sb = new StringBuilder(
            " NAME                              " +
            "passed in count | 80-81 | 82-83 | 84-85 | 86-87 | 88-89  ..... 99-100 | Оценка" +
            "\r\n----------------------------------------------------------" +
            "-------------------------------");
        foreach (var ts in res) {
            sb.Append("\r\n" + ts.exam.PadRight(maxNameLen) + $" ({ts.percent:000}% in {ts.count:000}) :");
            for (int i = 80; i <= 100; i += 2) {
                var a = ts.hardness.FirstOrDefault(s => s.score == i && s.count > 10);
                if (a == null)
                    sb.AppendTableItem($"----");
                else
                    sb.AppendTableItem($"{a?.percent:00.0}");
            }
        }

        sb.Append("\r\nsimple questions:\r\n");
        foreach (var type in res.OrderBy(c => 100 * c.simplePassed / (double)c.simpleCount)) {
            sb.Append("\r\n" + type.exam.PadRight(maxNameLen) +
                      $" ({100 * type.simplePassed / (double)type.simpleCount:000}% in {type.count:000}) :");
        }

        var s = sb.ToString();
        return s;
    }

    public static double GetHardness(Qmodel qmodel) =>
        95 - 0.8 * Math.Log(qmodel.PreviousExamDeltaInSecs) + 0.5 * qmodel.ScoreBefore;

    public static string GetWeightedQuestionMetricsReport(List<Qmodel> allMetrics) {
        double GetScoreWeight(double i) =>
            1 - (1 / 28.0) * i;

        var sb = new StringBuilder();
        var groups = allMetrics.GroupBy(a => Helper.GetNonCleanName(a.ExamName));
        var results = new List<wBucket>();
        foreach (var examGroup in groups) {
            var passed = 0.0;
            var missed = 0.0;
            var countPassed = 0.0;
            var countMissed = 0.0;
            var simplePassed = 0.0;
            var simpleCount = 0.0;
            var hardPassed = 0.0;
            var hardCount = 0.0;
            var count = 0;

            foreach (var question in examGroup) {
                count++;

                var timeW = GetTimeWeight(question.PreviousExamDeltaInSecs) / 100.0;
                var scoreW = GetScoreWeight(question.ScoreBefore);
                //маленький вес если только что задан с высоким скором.
                //1 если давно задан с низким скором
                var wMissed = 0.2 + 0.8 * timeW * scoreW;
                var wPassed = 1 - wMissed;

                if (question.Result)
                    passed += wPassed;
                else
                    missed += wMissed;

                countPassed += wPassed;
                countMissed += wMissed;


                if (question.ScoreBefore <= 5 && question.QuestionsAsked < 20) {
                    var wMissedSimple = 0.2 + 0.8 * timeW;
                    var wPassedSimple = 1 - wMissedSimple;
                    if (question.Result)
                        simplePassed += wPassedSimple;
                    simpleCount += wPassedSimple;
                }

                if (question.ScoreBefore >= 6) {
                    var wMissedHard = 0.2 + 0.8 * timeW;
                    var wPassedHard = 1 - wMissedHard;
                    if (question.Result)
                        hardPassed += wPassedHard;
                    hardCount += wPassedHard;
                }
            }

            results.Add(new wBucket(examGroup.Key.PadRight(30), passed, missed, countPassed, countMissed, count,
                simplePassed, simpleCount, hardPassed, hardCount));
        }


        sb.Append("\r\nRELATIVES-------------\r\n");

        foreach (var r in results.Where(c => c.CountMissed > 300).OrderBy(r => r.RelativePassed)) {
            sb.Append($"{r.Name}{r.TotalCount:0000}:   {(r.RelativePassed * 1000):000}\r\n");
        }

        sb.Append("\r\nRELATIVES MISSED-------------\r\n");

        foreach (var r in results.Where(c => c.CountMissed > 300).OrderByDescending(r => r.RelativeMissed)) {
            sb.Append($"{r.Name}{r.TotalCount:0000}:   {(r.RelativeMissed * 1000):000}\r\n");
        }

        sb.Append("\r\nSIMPLE RELATIVES-------------\r\n");

        foreach (var r in results.Where(c => c.CountMissed > 300).OrderBy(r => r.RelativeSimplePassed)) {
            sb.Append($"{r.Name}{r.TotalCount:0000}:   {(r.RelativeSimplePassed * 1000):000}\r\n");
        }

        sb.Append("\r\nHARD RELATIVES-------------\r\n");

        foreach (var r in results.Where(c => c.CountMissed > 300).OrderBy(r => r.RelativeHardPassed)) {
            sb.Append($"{r.Name}{r.TotalCount:0000}:   {(r.RelativeHardPassed * 1000):000}\r\n");
        }

        return sb.ToString();
    }

    //100 = just right now. 0 = > very very long
    private static double GetTimeWeight(long seconds) => Math.Min(100, Math.Max(0, 100.0 - 1.5 * Math.Log(seconds)));


    class wBucket {
        public double HardPassed { get; }
        public double HardWeightedCount { get; }

        public wBucket(
            string name, double passed,
            double missed,
            double countPassed, double countMissed,
            int count,
            double simplePassed, double simpleCount,
            double hardPassed, double hardCount) {
            HardPassed = hardPassed;
            HardWeightedCount = hardCount;
            Name = name;
            Passed = passed;
            Missed = missed;
            CountPassed = countPassed;
            CountMissed = countMissed;
            TotalCount = count;
            SimplePassed = simplePassed;
            SimpleWeightedCount = simpleCount;
        }

        public int TotalCount { get; }
        public double SimplePassed { get; }
        public double SimpleWeightedCount { get; }
        public double CountMissed { get; }
        public double CountPassed { get; }
        public double Passed { get; }
        public double Missed { get; }
        public string Name { get; }

        public double RelativeHardPassed => HardPassed / HardWeightedCount;
        public double RelativeSimplePassed => SimplePassed / SimpleWeightedCount;
        public double RelativePassed => Passed / CountPassed;
        public double RelativeMissed => Missed / CountMissed;
    }

    class QuestionToTimeBucket {
        public QuestionToTimeBucket(string questionName) {
            QuestionName = questionName;
        }

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
            if (result) {
                Passed++;
                PassedByScore[item]++;
            }
        }

        public QuestionToTimeBucket Sum(QuestionToTimeBucket other, string name) {
            var result = new QuestionToTimeBucket(name)
            {
                Count = Count + other.Count,
                Passed = Passed + other.Passed,
            };
            for (int i = 0; i < CountByScore.Length; i++) {
                result.CountByScore[i] = CountByScore[i] + other.CountByScore[i];
                result.PassedByScore[i] = PassedByScore[i] + other.PassedByScore[i];
            }

            return result;
        }
    }
}
}