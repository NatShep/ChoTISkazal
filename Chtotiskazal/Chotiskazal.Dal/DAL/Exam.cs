using System;

namespace Chotiskazal.DAL
{
    public class Exam
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Started { get; set; }
        public  DateTime Finished { get; set; }

        public int Count { get; set; } 
        public int Passed { get; set; } 
        public int Failed { get; set; }
    }
}