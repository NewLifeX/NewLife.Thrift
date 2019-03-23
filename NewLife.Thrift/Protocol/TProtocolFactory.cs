using NewLife.Thrift.Transport;

namespace NewLife.Thrift.Protocol
{
    public interface TProtocolFactory
    {
        TProtocol GetProtocol(TTransport trans);
    }
}