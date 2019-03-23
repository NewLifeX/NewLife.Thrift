using NewLife.Thrift.Server;

namespace NewLife.Thrift
{
    public interface TControllingHandler
    {
        TServer server { get; set; }
    }
}