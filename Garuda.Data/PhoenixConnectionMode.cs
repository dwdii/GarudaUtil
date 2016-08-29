
namespace Garuda.Data
{
    public enum PhoenixConnectionMode
    {
        None = 0,
        HDInsightGateway = 1,
        VNET = 2,

    }

    public struct PhoenixConnectionModeStr
    {
        public const string Vnet = "vnet";
        public const string HdiGateway = "hdi-gateway";
    }
}