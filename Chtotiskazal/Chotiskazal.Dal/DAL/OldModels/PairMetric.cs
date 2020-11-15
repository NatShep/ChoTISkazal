﻿using System;
using System.Collections.Generic;
using System.Linq;
 using Chotiskazal.DAL;
 using Chotiskazal.Dal.Enums;
 using Chotiskazal.LogicR;

 namespace Chotiskazal.DAL
{
    public class PairMetric
    {
        public const int MaxExamScore = 10;
        public const int PenaltyScore = 9;

        private const int ExamFailedPenalty = 2;
        private const double AgingFactor = 1;
        public const double ReducingPerPointFactor = 1.7;


        public long MetricId { get; set; }
        public string EnWord { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }

        public int PassedScore { get; set; }
        public double AggregateScore { get; set; }

        public DateTime LastExam { get; set; }
        public int Examed { get; set; }

        public int Revision { get; set; }

    }
}