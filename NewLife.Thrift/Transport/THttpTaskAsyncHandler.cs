using System.Threading.Tasks;
using System.Web;
using NewLife.Thrift.Protocol;

namespace NewLife.Thrift.Transport
{
#if !__CORE__ && !NET40
    /// <summary>
    /// An async task based HTTP handler for processing thrift services.
    /// </summary>
    public class THttpTaskAsyncHandler : HttpTaskAsyncHandler
    {
        private readonly TAsyncProcessor _processor;
        private readonly TProtocolFactory _inputProtocolFactory;
        private readonly TProtocolFactory _outputProtocolFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="THttpTaskAsyncHandler"/> class
        /// using the <see cref="TBinaryProtocol.Factory"/> for both input and output streams.
        /// </summary>
        /// <param name="processor">The async processor implementation.</param>
        public THttpTaskAsyncHandler(TAsyncProcessor processor)
            : this(processor, new TBinaryProtocol.Factory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="THttpTaskAsyncHandler"/> class
        /// using <paramref name="protocolFactory"/> for both input and output streams.
        /// </summary>
        /// <param name="processor">The async processor implementation.</param>
        /// <param name="protocolFactory">The protocol factory.</param>
        public THttpTaskAsyncHandler(TAsyncProcessor processor, TProtocolFactory protocolFactory)
            : this(processor, protocolFactory, protocolFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="THttpTaskAsyncHandler"/> class.
        /// </summary>
        /// <param name="processor">The async processor implementation.</param>
        /// <param name="inputProtocolFactory">The input protocol factory.</param>
        /// <param name="outputProtocolFactory">The output protocol factory.</param>
        public THttpTaskAsyncHandler(TAsyncProcessor processor, TProtocolFactory inputProtocolFactory,
            TProtocolFactory outputProtocolFactory)
        {
            _processor = processor;
            _inputProtocolFactory = inputProtocolFactory;
            _outputProtocolFactory = outputProtocolFactory;
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var transport = new TStreamTransport(context.Request.InputStream, context.Response.OutputStream);

            try
            {
                var input = _inputProtocolFactory.GetProtocol(transport);
                var output = _outputProtocolFactory.GetProtocol(transport);

                while (await _processor.ProcessAsync(input, output))
                {
                }
            }
            catch (TTransportException)
            {
                // Client died, just move on
            }
            finally
            {
                transport.Close();
            }
        }
    }
#endif
}