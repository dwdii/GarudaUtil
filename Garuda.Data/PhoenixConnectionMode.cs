
namespace Garuda.Data
{
    /// <summary>
    /// Specifies a value for the Mode property of the PhoenixConnection class.
    /// </summary>
    public enum PhoenixConnectionMode
    {
        /// <summary>
        /// The value has not been specified 
        /// </summary>
        None = 0,
        /// <summary>
        /// Indicates a connection mode associated with an HDInsight Gateway
        /// </summary>
        HDInsightGateway = 1,
        /// <summary>
        /// Indicates a connection mode associated with a Virtual Network or internal access
        /// to a Phoenix Query Server.
        /// </summary>
        VNET = 2,
    }

    /// <summary>
    /// Specifies the constants associated with the Mode property of the PhoenixConnection's 
    /// ConnectionString property.
    /// </summary>
    public struct PhoenixConnectionModeStr
    {
        /// <summary>
        /// Indicates a connection mode associated with a Virtual Network or internal access
        /// to a Phoenix Query Server.
        /// </summary>
        public const string Vnet = "vnet";

        /// <summary>
        /// Indicates a connection mode associated with an HDInsight Gateway
        /// </summary>
        public const string HdiGateway = "hdi-gateway";
    }
}