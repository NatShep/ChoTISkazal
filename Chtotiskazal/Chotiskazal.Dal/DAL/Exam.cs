using Chotiskazal.DAL;
using System;

namespace Chotiskasal.DAL
{
    public class Exam
    {
        public User User { get; set; }
        public DateTime Started { get; set; }
        public  DateTime Finished { get; set; }

        public int Count { get; set; } 
        public int Passed { get; set; } 
        public int Failed { get; set; }
    }
}