using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garuda.Data.MetaData
{
    /// <summary>
    /// Interface used to enable common handling of meta data across Phoenix objects.
    /// </summary>
    public interface IGarudaPhoenixMetaData
    {
        /// <summary>
        /// Gets and sets the DataRow associated with the Phoenix object's meta data. This would
        /// get populated by the rows related to the DataTable returned from GarudaPhoenixTable.GetColumns for example.
        /// </summary>
        DataRow Row { get; set; }
    }
}
