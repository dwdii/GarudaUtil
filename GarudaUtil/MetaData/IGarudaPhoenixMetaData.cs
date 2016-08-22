using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarudaUtil.MetaData
{
    interface IGarudaPhoenixMetaData
    {
        DataRow Row { get; set; }
    }
}
