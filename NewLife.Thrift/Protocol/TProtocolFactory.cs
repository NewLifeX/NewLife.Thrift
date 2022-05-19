using NewLife.Thrift.Transport;

namespace NewLife.Thrift.Protocol
{
    /// <summary>
    /// 协议工厂
    /// </summary>
    public interface TProtocolFactory
    {
        /// <summary>
        /// 获取协议
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        TProtocol GetProtocol(TTransport trans);
    }
}