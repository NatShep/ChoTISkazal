using System;
using Chotiskazal.Dal.Services;


namespace Chotiskazal.DAL
{
    // хотим ли мы разделять слово-значение1, слово-значение2
    // или слово - все значения, помеченные юзером.
    public class UsersPair
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public int UserId { get; set; }
        public int PairId { get; set; }
        public bool IsPhrase { get; set; }
        public int MetricId { get; set; }

        public UsersPair(int userId, int pairId, int metricId, bool isPhrase)
        {
            Created = DateTime.Now;
            UserId = userId;
            PairId = pairId;
            IsPhrase = isPhrase;
            MetricId = metricId;
        }
        
        public UsersPair()
        {}

    }
    
}

