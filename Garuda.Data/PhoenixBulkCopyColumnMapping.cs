using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garuda.Data
{
    /// <summary>
    /// Defines the mapping between a column in a PhoenixBulkCopy instance's data source and a column in the instance's destination table.
    /// </summary>
    public class PhoenixBulkCopyColumnMapping
    {
        /// <summary>
        /// Creates a new column mapping with a default value for the column. 
        /// </summary>
        /// <param name="defaultValue"></param>
        public PhoenixBulkCopyColumnMapping(string defaultValue)
        {
            if(null == defaultValue)
            {
                throw new ArgumentNullException(nameof(defaultValue));
            }

            this.DefaultValue = defaultValue;
        }

        /// <summary>
        /// Gets or sets the default value for a column. For example a sequence next value or timestamp value.
        /// </summary>
        public string DefaultValue { get; set; }
    }
}
