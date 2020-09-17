namespace Chotiskazal.Dal.Migrations
{ 
public class CreateQuestionMetricTable : SimpleMigration
    {
        public override string Name => "CreateQuestionMetricTable";

        public override string  Query => @"create table if not exists QuestionMetric
              (
                 Id                                  integer primary key AUTOINCREMENT,
                 UserPairId                          integer,
                 ElapsedMs                           integer,
                 Result                              integer,
                 Type                                nvarchar(100) not null,
                 Revision                           integer,
                 Translation                         nvarchar(100) not null,
                 PreviousExam                        datetime not null,
                 LastExam                            datetime not null,
                 ExamPassed                          integer not null,
                 Examed                              integer not null,
                 PassedScore                         integer not null,
                 AggregateScore                      real not null,
                 AggregateScoreBefore                real not null,
                 PassedScoreBefore                   real not null 
              )";
    }
}