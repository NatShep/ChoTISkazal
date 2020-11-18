using Dapper;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.Dal.DAL;

namespace Chotiskazal.Dal.Repo
{
    public class UserWordsRepo : BaseRepo
    {
        public UserWordsRepo(string fileName) : base(fileName) 
        {
        }

        public async Task<int> SaveToUserDictionaryAsync(UserWordForLearning userWordForLearning)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return await cnn.ExecuteScalarAsync<int>(
                    @"INSERT INTO UserWords (UserId, EnWord, UserTranslations, Transcription, Created, PhrasesIds, IsPhrase,
                                                 PassedScore, AggregateScore,LastExam,Examed,Revision)

                      VALUES(@UserId,  @EnWord, @UserTranslations, @Transcription, @Created, @PhrasesIds, @IsPhrase,
                                                 @PassedScore, @AggregateScore,@LastExam,@Examed,@Revision)",
                    userWordForLearning);
            }
        }

        public async Task<UserWordForLearning[]> GetWorstForUserWithPhrasesAsync(int userId, int count)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();

            using (var transaction = cnn.BeginTransaction())
            {
                var result = (await cnn.QueryAsync<UserWordForLearning>(
                    @"Select * FROM UserWords WHERE UserId=@userId               
				  order by AggregateScore desc limit @count          		
                ", new {userId, count},transaction)).ToArray();

                foreach (var userWordForLearning in result)
                {
                    var phrases = new List<Phrase>();
                    foreach (var translation in userWordForLearning.GetTranslations())
                    {
                        phrases.AddRange(cnn.Query<Phrase>(
                            @"Select * FROM Phrases WHERE EnWord=@EnWord AND WordTranslate=@translation              		
                            ", new {userWordForLearning.EnWord, translation},transaction));
                    }

                    userWordForLearning.Phrases = phrases;
                }
                transaction.Commit();
                return result.ToArray();
            }
        }

        public async Task<UserWordForLearning[]> GetWorstTestWordsWithPhrasesForUserAsync(int count, int learnRate, int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            {
                cnn.Open();
                using (var transaction = cnn.BeginTransaction())
                {
                    var result = (await cnn.QueryAsync<UserWordForLearning>(
                        @"Select * FROM UserWords WHERE UserId=@userId AND PassedScore > @learnRate 
                  order by AggregateScore desc limit @count       		
                ", new {userId, learnRate, count}, transaction)).ToArray();

                    foreach (var userWordForLearning in result)
                    {
                        var phrases = new List<Phrase>();
                        foreach (var translation in userWordForLearning.GetTranslations())
                        {
                            phrases.AddRange(cnn.Query<Phrase>(
                                @"Select * FROM Phrases WHERE EnWord=@EnWord AND WordTranslate=@translation              		
                            ", new {userWordForLearning.EnWord, translation}, transaction));
                        }

                        userWordForLearning.Phrases = phrases;
                    }

                    transaction.Commit();
                    return result.ToArray();
                }
            }
        }

        public async Task<UserWordForLearning[]> GetAllUserWordsAsync(int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            {
                cnn.Open();
                return (await cnn.QueryAsync<UserWordForLearning>(
                    @"Select * from UserWords WHERE UserId=userId
                ", new {userId})).ToArray();
            }
        }

        public async Task<string[]> GetAllEnWordsForUserAsync(int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            {
                cnn.Open();
                return (await cnn.QueryAsync<string>(
                    @"Select EnWord FROM UserWords where UserId==@userId
                  ", new {userId})).ToArray();
            }
        }

        public async Task UpdateScoresAsync(UserWordForLearning userWordForLearning)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var op =
                    $"Update UserWords set AggregateScore = @AggregateScore, PassedScore = @PassedScore," +
                    $"LastExam = @LastExam," +
                    $"Examed = @Examed " +
                    $"WHERE Id = @Id";
                await cnn.ExecuteAsync(op, userWordForLearning);
            }
        }

        public async Task UpdateAgingAndRandomizationAsync(int userId, int count)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                foreach (var word in cnn.Query<UserWordForLearning>
                    (@"Select * From UserWords WhERE UserId=@userId order by RANDOM() limit @count", 
                    new {userId, count}).ToArray())
                {
                    word.UpdateAgingAndRandomization();
                    var op = $"Update UserWords set AggregateScore = " +
                             $"{word.AggregateScore.ToString(CultureInfo.InvariantCulture)} " +
                             $"where Id = {word.Id}";
                    await cnn.ExecuteAsync(op);
                }
            }
        }

        public async Task<UserWordForLearning> GetWordForLearningByEnWordOrNullAsync(int userId, string enWord)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            {
                cnn.Open();

                return (await cnn.QueryAsync<UserWordForLearning>(
                    @"Select * FROM UserWords WHERE UserId=@userId AND enWord = @enWord       		
                ", new {userId, enWord})).Single();
            }
        }

        public async Task<UserWordForLearning> GetAnyWordAsync(int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            {
                cnn.Open();

                return (await cnn.QueryAsync<UserWordForLearning>(
                    @"Select * FROM UserWords Where UserId =@userId Limit 1", new {userId})).Single();
            }
        }

        public void UpdateWordTranslations(UserWordForLearning userWord)
        {

            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();

                var op =
                        $"Update UserWords Set " +
                        $" UserTranslations = @UserTranslations," +
                        $"PhrasesIds = @PhrasesIds " +
                        $"Where Id = @Id";
                    cnn.Execute(op,userWord);

            }
        }

    
    }
}