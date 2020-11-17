using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Chotiskazal.Dal.DAL;
using Chotiskazal.Dal.Enums;
using Dapper;

namespace Chotiskazal.Dal.Repo
{
    public class DictionaryRepository : BaseRepo
    {
        public DictionaryRepository(string fileName) : base(fileName){}
        
        public async Task<int> AddWordPairAsync(WordDictionary word)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return await cnn.ExecuteScalarAsync<int>(
                    @"INSERT INTO PairDictionary (  EnWord,  Transcription,  RuWord, Sourse)
                                      VALUES( @EnWord,  @Transcription,  @RuWord, @Source); 
                          select last_insert_rowid()", word);
            }
        }
        
        public async Task<int> AddPhraseAsync(int pairId, string enWord, string ruWord, string enPhrase, string ruTranslate)
        {
            CheckDbFile(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                var phrase = new Phrase(pairId, enWord, ruWord , enPhrase, ruTranslate);
                cnn.Open();
                 return await cnn.ExecuteScalarAsync<int>(
                    @"INSERT INTO Phrases ( PairId , EnWord, WordTranslate, EnPhrase, PhraseRuTranslate)
                                      VALUES( @PairId,  @EnWord, @WordTranslate, @EnPhrase,  @PhraseRuTranslate); 
                          select last_insert_rowid()", phrase);
            }
        }
        
        public async Task<string[]> GetAllTranslateAsync(string word)
        {
            CheckDbFile(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return  (await cnn.QueryAsync<string>(
                    @"SELECT RuWord FROM PairDictionary  WHERE  EnWord=@word", new {word})).ToArray();
            }
        }
        
        public async Task<Phrase[]> FindPhrasesBySomeIdsForUserAsync(int[] allPhrasesIdForUser)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                
                return (await  cnn.QueryAsync<Phrase>(
                    @"SELECT * FROM Phrases 
                    WHERE PairId in @allPhrasesIdForUser", new {allPhrasesIdForUser})).ToArray();
            }
        }

        public async Task<int[]> AddWordPairWithPhrasesAsync(string enWord, string yandexTranslationText, 
            string transcription, TranslationSource yadic, List<Phrase> yaPhrases)
        {
            
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                using (var transaction = cnn.BeginTransaction())
                {
                    var phrasesIds = new List<int>();
                    var word = new WordDictionary(enWord, yandexTranslationText, transcription, yadic, yaPhrases);
                    var id = await cnn.ExecuteScalarAsync<int>(
                        @"INSERT INTO PairDictionary (  EnWord,  Transcription,  RuWord, Sourse)
                                      VALUES( @EnWord,  @Transcription,  @RuWord, @Source); 
                          select last_insert_rowid()", word,transaction:transaction);
                    foreach (var phrase in yaPhrases)
                    {
                        phrase.PairId = id;
                        var phraseId = await cnn.ExecuteScalarAsync<int>(
                            @"INSERT INTO Phrases ( PairId , EnWord, WordTranslate, EnPhrase, PhraseRuTranslate)
                                      VALUES( @Pairid,  @EnWord, @WordTranslate, @EnPhrase,  @PhraseRuTranslate); 
                          select last_insert_rowid()", phrase, transaction: transaction);
                        phrase.Id = phraseId;
                    }
                    var a = yaPhrases;
                    transaction.Commit();
                    return phrasesIds.ToArray();
                }
            }
        }

        public async Task<WordDictionary[]> GetPairsWithPhrasesByWordOrNullAsync(string word)
        {
            //TODO find by RuWord or by EnWord.
            //if word == RuWord(английские символы)
            //if word == EnWord(русские символы)
            
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                using (var transaction = cnn.BeginTransaction())
                {
                    var words= (await cnn.QueryAsync<WordDictionary>(
                        @"SELECT * FROM PairDictionary WHERE EnWord = @word", new {word},transaction)).ToArray();

                    foreach (var wordForLearning in words)
                    {
                        wordForLearning.Phrases = (await cnn.QueryAsync<Phrase>(
                                @"Select * From Phrases WHERE PairId = @PairId",
                                new {wordForLearning.PairId},transaction)).ToList();
                    }
                    transaction.Commit();
                    return words.ToArray();
                }
            }
        }
    }
}
