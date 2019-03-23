using NewLife.Thrift.Server;
using NewLife.Thrift.Transport;

namespace NewLife.Thrift
{
    public class TSingletonProcessorFactory : TProcessorFactory
    {
        private readonly TProcessor processor_;

        public TSingletonProcessorFactory(TProcessor processor) => processor_ = processor;

        public TProcessor GetProcessor(TTransport trans, TServer server = null) => processor_;
    }
}