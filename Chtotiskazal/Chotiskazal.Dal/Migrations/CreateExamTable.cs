namespace Chotiskazal.Dal.Migrations
{
    public class CreateExamTable : SimpleMigration
    {
        public override string Name => "CreateExamTable";

        public override string  Query => @"create table if not exists Exams
              (
                 Id                                  integer primary key AUTOINCREMENT,
                 UserId                              integer not null,           
                 Started                             datetime not null,
                 Finished                            datetime not null,
                 Count                                integer,
                 Passed                              integer not null,           
                 Failed                              integer not null           
              )";
    }
}