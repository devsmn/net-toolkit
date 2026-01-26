using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devsmn.Common.Data.SQLite
{
    public class SqliteResult
    {
        public long RowId { get; set; }
    }

    public class SqliteResult<TData>
    {
        public TData? Data { get; set; }
    }
}
