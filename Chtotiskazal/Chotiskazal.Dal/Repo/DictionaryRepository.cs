using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Chotiskazal.DAL;
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
                                      VALUES( @EnWord,  @Transcription,  @RuWord, @Sourse); 
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

        public async Task<WordDictionary[]> GetAllWordPairsByWordAsync(string word)
        {
            //TODO find by RuWord or by EnWord.
            //Check the symbol of word before seeking

            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return (await cnn.QueryAsync<WordDictionary>(
                    @"SELECT * FROM PairDictionary WHERE EnWord = @word", new {word})).ToArray();
                
            }
        }
        
        public async Task<Phrase[]> GetAllPhrasesByPairIdAsync(int wordPairId)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return(await cnn.QueryAsync<Phrase>(@"Select * From Phrases WHERE PairId = @wordPairId",new{wordPairId}))
                    .ToArray();
            }
        }

        public async Task<WordDictionary> GetPairWithPhrasesByIdOrNullAsync(int id)
        {
            CheckDbFile(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var phrases = new List<Phrase>();
                var word= (await cnn.QueryAsync<WordDictionary, Phrase, WordDictionary>(
                    @"SELECT * FROM 
                    (SELECT * FROM PairDictionary WHERE PairId=@id) pd
                     JOIN Phrases ph ON ph.PairId = pd.PairId",
                    (pd, ph) =>
                    {
                        //TODO почему он не мапит Id для фразы???
                       phrases.Add(ph);
                       return pd;
                    }, new {id},
                    splitOn:"PairId")).FirstOrDefault();
                if (word == null)
                    return null;
                word.Phrases = phrases;
                return word;
            }
            return null;
        }
        
        public async Task<string[]> GetAllTranslateAsync(string word)
        {
            //TODO find by RuWord or by EnWord.
            //if word == RuWord(английские символы), ищем в таблице WordPairDictionary по русскому слову все переводы
            //if word == EnWord(русские символы), ищем в таблице WordPairDictionary по английскому слову все переводы
          
            CheckDbFile(DbFile);
            //TODO Is it need distinct?
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return  (await cnn.QueryAsync<string>(
                    @"SELECT RuWord FROM PairDictionary  WHERE  EnWord=@word", new {word})).ToArray();
            }
        }

        public async Task RemovePhraseAsync(int phraseId)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var sqlQuery = "DELETE FROM Phrases WHERE Id = @phrasesId";
                await cnn.ExecuteAsync(sqlQuery, new { phraseId });
            }
        }
        
        public async Task<Phrase[]> FindSeveralPhraseByIdAsy(int[] allPhrasesIdForUser)
        {
            CheckDbFile(DbFile);

            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                return (await  cnn.QueryAsync<Phrase>(
                    @"SELECT * FROM Phrases 
                    WHERE PairId in @allPhrasesIdForUser", allPhrasesIdForUser)).ToArray();
            }
        }
        
        //TODO Additional methods 
        public WordDictionary GetPairByIdOrNull(int? id)
        {
            CheckDbFile(DbFile);
            
            using (var cnn = SimpleDbConnection())
            {
                cnn.Open();
                var result = cnn.Query<WordDictionary>(
                    @"SELECT * FROM PairDictionary 
                    WHERE PairId = @id", new {id}).FirstOrDefault();
                return result;
            }
        }

      
    }
}
