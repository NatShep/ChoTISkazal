namespace Chotiskazal.Dal.Migrations
{ 
public class CreatePairDictionaryTable : SimpleMigration
    {
        public override string Name => "PairDictionary Table";

        public override string  Query => @"create table if not exists PairDictionary
              (
                PairId                                  integer primary key AUTOINCREMENT,
                EnWord                              nvarchar(100) not null,
                Transcription                       nvarchar(100) not null,
                RuWord                              nvarchar(100) not null,
                Sourse                              nvarchar(100) not null
              )";
    }
}