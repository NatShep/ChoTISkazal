namespace Chotiskazal.Dal.Migrations
{
    public class CreatePairMetricTable : SimpleMigration
    {
        public override string Name => "CreatePairMetricsTable";

        public override string  Query => @"create table if not exists PairMetrics
              (
                 MetricId                            integer primary key AUTOINCREMENT,
                 EnWord                              string not null,
                 UserId                              integer not null,
                 Created                             datetime not null,
                 PassedScore                         real not null,             
                 AggregateScore                      double ,
                 LastExam                            integer not null,               
                 Examed                              integer,
                 Revision                            int not null               
              )";
        
    }
}