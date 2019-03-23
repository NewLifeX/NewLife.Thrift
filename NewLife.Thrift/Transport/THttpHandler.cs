using System;
using System.IO;
using System.Net;
using System.Web;
using NewLife.Thrift.Protocol;

namespace NewLife.Thrift.Transport
{
#if !__CORE__
    public class THttpHandler : IHttpHandler
    {
        protected TProcessor processor;

        protected TProtocolFactory inputProtocolFactory;
        protected TProtocolFactory outputProtocolFactory;

        protected const String contentType = "application/x-thrift";
        protected System.Text.Encoding encoding = System.Text.Encoding.UTF8;

        public THttpHandler(TProcessor processor)
            : this(processor, new TBinaryProtocol.Factory())
        {

        }

        public THttpHandler(TProcessor processor, TProtocolFactory protocolFactory)
            : this(processor, protocolFactory, protocolFactory)
        {

        }

        public THttpHandler(TProcessor processor, TProtocolFactory inputProtocolFactory, TProtocolFactory outputProtocolFactory)
        {
            this.processor = processor;
            this.inputProtocolFactory = inputProtocolFactory;
            this.outputProtocolFactory = outputProtocolFactory;
        }

        public void ProcessRequest(HttpListenerContext context)
        {
            context.Response.ContentType = contentType;
            context.Response.ContentEncoding = encoding;
            ProcessRequest(context.Request.InputStream, context.Response.OutputStream);
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = contentType;
            context.Response.ContentEncoding = encoding;
            ProcessRequest(context.Request.InputStream, context.Response.OutputStream);
        }

        public void ProcessRequest(Stream input, Stream output)
        {
            TTransport transport = new TStreamTransport(input, output);

            try
            {
                var inputProtocol = inputProtocolFactory.GetProtocol(transport);
                var outputProtocol = outputProtocolFactory.GetProtocol(transport);

                while (processor.Process(inputProtocol, outputProtocol))
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

        public Boolean IsReusable
        {
            get { return true; }
        }
    }
#endif
}