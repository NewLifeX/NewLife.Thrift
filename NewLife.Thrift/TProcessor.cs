using System;
using NewLife.Thrift.Protocol;

namespace NewLife.Thrift
{
    public interface TProcessor
    {
        Boolean Process(TProtocol iprot, TProtocol oprot);
    }
}