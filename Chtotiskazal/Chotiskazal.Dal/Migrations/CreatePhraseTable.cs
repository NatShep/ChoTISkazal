namespace Chotiskazal.Dal.Migrations
{
    public class CreatePhraseTableMigration : SimpleMigration
    {
        public override string Name => "CreatePhraseTable";

        public override string  Query => @"create table if not exists Phrases
              (
                 Id                                  integer primary key AUTOINCREMENT,
                 PairId                              integer not null,
                 EnPhrase                              nvarchar(100) not null,
                 RuTranslate                                nvarchar(100) not null,
                 FOREIGN KEY (PairId) REFERENCES UsersPair(Id)
              )";
    }
}