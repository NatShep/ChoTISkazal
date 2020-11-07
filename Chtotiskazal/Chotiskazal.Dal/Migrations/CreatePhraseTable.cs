namespace Chotiskazal.Dal.Migrations
{
    public class CreatePhraseTableMigration : SimpleMigration
    {
        public override string Name => "CreatePhraseTable";

        public override string  Query => @"create table if not exists Phrases
              (
                 Id                                  integer primary key AUTOINCREMENT,
                 PairId                              integer not null,
                 EnWord                              nvarchar(100) not null,
                 WordTranslate                              nvarchar(100) not null,
                 EnPhrase                            nvarchar(100) not null,
                 PhraseRuTranslate                         nvarchar(100) not null,
                 FOREIGN KEY (PairId) REFERENCES PairDictionary(PairId)
              )";
    }
}