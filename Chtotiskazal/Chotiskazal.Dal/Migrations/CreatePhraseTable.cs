namespace Chotiskazal.Dal.Migrations
{
    public class CreatePhraseTableMigration : SimpleMigration
    {
        public override string Name => "CreatePhraseTable";

        public override string  Query => @"create table if not exists Phrases
              (
                 Id                                  integer primary key AUTOINCREMENT,
                 PairId                              integer primary key AUTOINCREMENT,
                 Word                                nvarchar(100) not null,
                 EnPhrase                              nvarchar(100) not null,
                 RuTranslate                                nvarchar(100) not null,
              )";
    }
}