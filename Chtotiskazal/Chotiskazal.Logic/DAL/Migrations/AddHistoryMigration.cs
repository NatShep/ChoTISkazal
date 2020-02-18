namespace Chotiskazal.Logic.DAL.Migrations
{
    public class AddHistoryMigration : SimpleMigration
    {
        public override string Name => "AddHistory";

        public override string Query => 
            @"create table if not exists ExamHistory
              (
                 Id                                  integer primary key AUTOINCREMENT,
                 Count                               integer not null,
                 Passed                              integer not null,
                 Failed                              integer not null,
                 Started                             datetime not null,
                 Finished                            datetime null
            )";
    }
}