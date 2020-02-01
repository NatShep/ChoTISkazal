using System;

namespace Dic.Logic.DAL
{
    public class Exam
    {
        public DateTime Started
        {
            get;
            set;
        }
        public  DateTime Finished { get; set; }
        public int Count { get; set; } 
        public int Passed { get; set; } 
        public int Failed { get; set; }
    }
}