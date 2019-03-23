using NewLife.Thrift.Server;
using NewLife.Thrift.Transport;

namespace NewLife.Thrift
{
    public interface TProcessorFactory
    {
        TProcessor GetProcessor(TTransport trans, TServer server = null);
    }
}