using Apache.Phoenix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garuda.Data
{
    internal class GarudaExecuteResponse
    {
        public ExecuteResponse Response { get; set; }

        public uint StatementId { get; set; }
    }
}
