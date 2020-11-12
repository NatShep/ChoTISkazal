using Chotiskazal.DAL;
using Dapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //TODO how is better to do
            var result =(await cnn.QueryAsync<UserWordForLearning>(
                @"Select * FROM UserWords WHERE UserId=@userId               
				  order by AggregateScore desc limit @count          		
                ", new {userId, count})).ToArray();
            foreach (var userWordForLearning in result)
            {
                var phrases = new List<Phrase>();
                foreach (var translation in userWordForLearning.GetTranslations())
                {
                    phrases.AddRange(cnn.Query<Phrase>(
                        @"Select * FROM Phrases WHERE EnWord=@EnWord AND WordTranslate=@translation              		
                ", new {userWordForLearning.EnWord, translation}));
                }

                userWordForLearning.Phrases = phrases;
            }
            return result.ToArray();
        }

        public async Task<UserWordForLearning[]> GetWorstTestWordsForUserAsync(int count, int learnRate, int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();

            return (await cnn.QueryAsync<UserWordForLearning>(
                @"Select * FROM UserWords WHERE UserId=@userId AND PassedScore > @learnRate 
                  order by AggregateScore desc limit @count       		
                ", new {userId, learnRate, count})).ToArray();
        }

        public async Task<UserWordForLearning[]> GetAllUserWordsForLearningAsync(int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();
            return ( await  cnn.QueryAsync<UserWordForLearning>(
                @"Select * from UserWords WHERE UserId=userId
                ", new {userId})).ToArray();
        }

        public async Task<string[]> GetAllWordsForUserAsync(int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();
            return (await cnn.QueryAsync<string>(
                @"Select EnWord FROM UsersPairs where up.UserId==@userId
                  ", new {userId})).ToArray();
        }

        public async Task UpdateScoresAsync(UserWordForLearning userWordForLearning)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var op =
                    $"Update UserWords set AggregateScore = @AggregateScore, "+//" WHERE Id =@Id";
                    $"PassedScore = @PassedScore," +
                    $"LastExam = @LastExam," +
                    $"Examed = @Examed " +
                    $"WHERE Id = @Id";
                await cnn.ExecuteAsync(op, userWordForLearning);
            }
        }

        public async Task UpdateAgingAndRandomizationAsync(int count)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                foreach (var word in cnn.Query<UserWordForLearning>
                    (@"Select * From UserWords order by RANDOM() limit @count", new {count}).ToArray())
                {
                    word.UpdateAgingAndRandomization();
                    var op = $"Update UserWords set AggregateScore = " +
                             $"{word.AggregateScore.ToString(CultureInfo.InvariantCulture)} " +
                             $"where Id = {word.Id}";
                    await cnn.ExecuteAsync(op);
                }
            }
        }

        public async Task<UserWordForLearning> GetWordByEnWordOrNullAsync(int userId, string enWord)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();

            return (await  cnn.QueryAsync<UserWordForLearning>(
                @"Select * FROM UserWords WHERE UserId=@userId AND enWord = @enWord       		
                ", new {userId,enWord})).FirstOrDefault();
        }

        public async Task<UserWordForLearning> GetAnyWordAsync(int userId)
        {
            CheckDbFile(DbFile);

            using var cnn = SimpleDbConnection();
            cnn.Open();

            return (await  cnn.QueryAsync<UserWordForLearning>(
                @"Select * FROM UserWords Where UserId =@userId Limit 1",new {userId})).FirstOrDefault();        
        }


        //TODO additional methods


        public void UpdateAgingAndRandomization()
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                foreach (var word in cnn.Query<UserWordForLearning>(@"Select * From UserPairs").ToArray())
                {
                    word.UpdateAgingAndRandomization();
                    var op =
                        $"Update UserWords set AggregateScore = {word.AggregateScore.ToString(CultureInfo.InvariantCulture)} where Id = {word.Id}";
                    cnn.Execute(op);
                }
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
                        $" UserTranslations = @UserTranslations " +
                        $"Where Id = @Id";
                    cnn.Execute(op,userWord);

            }
        }

    
    }
}