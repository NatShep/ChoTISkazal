using Chotiskazal.DAL;
using Dapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chotiskazal.Dal.Repo
{
    public class UserPairsRepo : BaseRepo
    {
        public UserPairsRepo(string fileName) : base(fileName)
        {
        }

        public async Task<int> SaveToUserDictionaryAsync(int pairId, int userId)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                using (var transaction = cnn.BeginTransaction())
                {
                    var metricId = await cnn.ExecuteScalarAsync<int>(
                        @"INSERT INTO QuestionMetric (ElaspedMs, Result, Type, Revision, PreviousExam, 
                          LastExam, ExamsPassed, Examed, PassedScore, AggregateScore, AggregateScoreBefore, PassedScoreBefore)
                          VALUES(@ElaspedMs, @Result, @Type, @Revision, @PreviousExam, @LastExam, @ExamsPassed, 
                          @Examed, @PassedScore, @AggregateScore, @AggregateScoreBefore, @PassedScoreBefore); 
                          select last_insert_rowid()", new QuestionMetric());

                    var result = await cnn.ExecuteScalarAsync<int>(
                        @"INSERT INTO UserPairs ( UserId ,  PairId, MetricId, Created, IsPhrase)
                      VALUES( @UserId,  @PairId, @MetricId, @Created, @IsPhrase); 
                          select last_insert_rowid()", new UserPair(userId, pairId, metricId, false));

                    transaction.Commit();
                    return result;
                }
            }
        }

        public async Task<UserPair> GetPairByDicIdOrNullAsync(int userId, int id)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return (await cnn.QueryAsync<UserPair>(
                    @"SELECT * FROM UserPairs 
                    WHERE UserId = @UserId AND PairId=@id", new {userId, id})).FirstOrDefault();
            }
        }

        public async  Task<string[]> GetAllTranslatesForWordForUserAsync(int userId, string word)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();

                return (await cnn.QueryAsync<string>(
                    @"SELECT pd.RuWord FROM PairDictionary pd
                    JOIN UserPairs up on up.PairId=pd.PairId 
                    WHERE up.UserId = @UserId AND pd.EnWord=@word", new {userId, word})).ToArray();
            }
        }

        public async Task<UserPair[]> GetWorstForUserAsync(int userId, int count)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();

            var lookup = new Dictionary<string, UserPair>();

            //TODO make easier when change db(adding word to userPairs)
            //Then you don't need PairDicionary
            await cnn.QueryAsync<UserPair, QuestionMetric, WordDictionary, UserPair>(
                @"Select * FROM 
                 (Select * FROM UserPairs WHERE UserId=@userId) up
                  JOIN	QuestionMetric q on q.MetricId=up.MetricId
	              JOIN PairDictionary dw on dw.PairId=up.PairId	 
				  order by q.AggregateScore desc limit @count          		
                ",
                (up, dw, q) =>
                {
                    var p = up.Created;
                    if (!lookup.TryGetValue(q.EnWord, out var pair))
                        lookup.Add(q.EnWord, up);
                    return up;
                }, new {userId, count},
                splitOn: "MetricId,PairId"
            );

            return lookup.Values.ToArray();
        }

        public async Task<UserPair[]> GetWorstTestWordsForUserAsync(int count, int learnRate, int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();

            var lookup = new Dictionary<string, UserPair>();

            //TODO make easier when change db(adding word to userPairs)
            //Then you don't need PairDicionary
            //TODO it's the same with GetWorstForUser sqlQuestion except LearnRate!!!
            //make it easier using GetWorstForUser method + aggregate by learnRate
            await cnn.QueryAsync<UserPair, QuestionMetric, WordDictionary, UserPair>(
                @"Select * FROM 
                 (Select * FROM UserPairs WHERE UserId=@userId) up
                  JOIN	QuestionMetric q on q.MetricId=up.MetricId
	              JOIN PairDictionary dw on dw.PairId=up.PairId	 
				  where PassedScore > @learnRate order by AggregateScore desc limit @count       		
                ",
                (up, dw, q) =>
                {
                    var p = up.Created;
                    if (!lookup.TryGetValue(q.EnWord, out var pair))
                        lookup.Add(q.EnWord, up);
                    return up;
                }, new {userId, learnRate, count},
                splitOn: "MetricId,PairId"
            );

            return lookup.Values.ToArray();

        }

        public async Task<string[]> GetAllWordsForUserAsync(int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();
            return (await cnn.QueryAsync<string>(
                @"Select DISTINCT EnWord 
                  FROM UsersPairs up where up.UserId==@userId
                  LEFT JOIN PairDictionary pd on up.PairId=pd.Id", new {userId})).ToArray();
        }

        public async  Task<Phrase[]> GetAllPhrasesAsync( int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();
            return (await cnn.QueryAsync<Phrase>(
                @"Select  * FROM Phrases p where (UserId==@userId) 
                 LEFT JOIN (Select * FROM Users WHERE UserId=@userId) u on u.PairId=p.PairId", new {userId})).ToArray();
        }
        
        //TODO additional methods
        public UserPair[] GetAllPairsForUser(int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();
            return cnn.Query<UserPair>(
                @"Select DISTINCT EnWord, from 
                (SELECT PairId FROM UsersPairs up where UserId==@userId 
                LEFT JOIN PairDictionare pd w on up.PairId=pd.Id", new {userId}).ToArray();
        }
    }
}