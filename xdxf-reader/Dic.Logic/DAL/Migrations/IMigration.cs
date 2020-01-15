using System;
using System.Collections.Generic;
using System.Text;

namespace Dic.Logic.DAL.Migrations
{
    interface IMigration
    {
        string Name { get; }
        string Query { get; }
    }
}
