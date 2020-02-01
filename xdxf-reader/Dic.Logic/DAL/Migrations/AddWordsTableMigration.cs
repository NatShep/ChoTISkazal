namespace Chotiskazal.Logic.DAL.Migrations
{
    public class AddWordsTableMigration : IMigration
    {
        public string Name => "AddWords";

        public string  Query => @"create table if not exists Words
              (
                 Id                                  integer primary key AUTOINCREMENT,
                 OriginWord                           nvarchar(100) not null,
                 Translation                          nvarchar(100) not null,
                 Created                              datetime not null,
                 LastExam                             datetime not null,
                 PassedScore integer not null,
                 Examed integer not null,
                 Transcription nvarchar(100) null,
                 AggregateScore real not null 
              )";
    }
}