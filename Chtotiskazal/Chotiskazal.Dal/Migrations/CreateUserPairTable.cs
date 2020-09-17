

namespace Chotiskazal.Dal.Migrations
{
    public class CreateUserPairTable : SimpleMigration
    {
        public override string Name => "CreateUserPairTable";

        public override string Query => @"create table if not exists UserPairs
              (
                 Id                                  integer primary key AUTOINCREMENT,
                 UserId                         integer,
                 PairId                        integer,
                 MetricId                            integer,
                 Created                             datetime not null,
                 IsPhrase                               bit      , 
                 FOREIGN KEY (Userid) REFERENCES Users(UserId)
              )";    }
}

