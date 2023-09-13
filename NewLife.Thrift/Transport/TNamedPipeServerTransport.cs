using System.IO.Pipes;
using System.Security.Principal;

namespace NewLife.Thrift.Transport
{
#if !NETSTANDARD
    public class TNamedPipeServerTransport : TServerTransport
    {
        /// <summary>
        /// This is the address of the Pipe on the localhost.
        /// </summary>
        private readonly String pipeAddress;
        private NamedPipeServerStream stream = null;
        private Boolean asyncMode = true;

        public TNamedPipeServerTransport(String pipeAddress)
        {
            this.pipeAddress = pipeAddress;
        }

        public override void Listen()
        {
            // nothing to do here
        }

        public override void Close()
        {
            if (stream != null)
            {
                try
                {
                    stream.Close();
                    stream.Dispose();
                }
                finally
                {
                    stream = null;
                }
            }
        }

        private void EnsurePipeInstance()
        {
            if (stream == null)
            {
                var direction = PipeDirection.InOut;
                var maxconn = NamedPipeServerStream.MaxAllowedServerInstances;
                var mode = PipeTransmissionMode.Byte;
                var options = asyncMode ? PipeOptions.Asynchronous : PipeOptions.None;
                const Int32 INBUF_SIZE = 4096;
                const Int32 OUTBUF_SIZE = 4096;

                // security
                var security = new PipeSecurity();
                security.AddAccessRule(
                    new PipeAccessRule(
                        new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                        PipeAccessRights.Read | PipeAccessRights.Write | PipeAccessRights.Synchronize | PipeAccessRights.CreateNewInstance,
                        System.Security.AccessControl.AccessControlType.Allow
                    )
                );

                try
                {
                    stream = new NamedPipeServerStream(pipeAddress, direction, maxconn, mode, options, INBUF_SIZE, OUTBUF_SIZE, security);
                }
                catch (NotImplementedException)  // Mono still does not support async, fallback to sync
                {
                    if (asyncMode)
                    {
                        options &= (~PipeOptions.Asynchronous);
                        stream = new NamedPipeServerStream(pipeAddress, direction, maxconn, mode, options, INBUF_SIZE, OUTBUF_SIZE, security);
                        asyncMode = false;
                    }
                    else
                    {
                        throw;
                    }
                }

            }
        }

        protected override TTransport AcceptImpl()
        {
            try
            {
                EnsurePipeInstance();

                if (asyncMode)
                {
                    var evt = new ManualResetEvent(false);
                    Exception eOuter = null;

                    stream.BeginWaitForConnection(asyncResult =>
                    {
                        try
                        {
                            if (stream != null)
                                stream.EndWaitForConnection(asyncResult);
                            else
                                eOuter = new TTransportException(TTransportException.ExceptionType.Interrupted);
                        }
                        catch (Exception e)
                        {
                            if (stream != null)
                                eOuter = e;
                            else
                                eOuter = new TTransportException(TTransportException.ExceptionType.Interrupted, e.Message, e);
                        }
                        evt.Set();
                    }, null);

                    evt.WaitOne();

                    if (eOuter != null)
                        throw eOuter; // rethrow exception
                }
                else
                {
                    stream.WaitForConnection();
                }

                var trans = new ServerTransport(stream, asyncMode);
                stream = null;  // pass ownership to ServerTransport
                return trans;
            }
            catch (TTransportException)
            {
                Close();
                throw;
            }
            catch (Exception e)
            {
                Close();
                throw new TTransportException(TTransportException.ExceptionType.NotOpen, e.Message, e);
            }
        }

        private class ServerTransport : TTransport
        {
            private NamedPipeServerStream stream;
            private readonly Boolean asyncMode;

            public ServerTransport(NamedPipeServerStream stream, Boolean asyncMode)
            {
                this.stream = stream;
                this.asyncMode = asyncMode;
            }

            public override Boolean IsOpen
            {
                get { return stream != null && stream.IsConnected; }
            }

            public override void Open()
            {
            }

            public override void Close()
            {
                stream?.Close();
            }

            public override Int32 Read(Byte[] buf, Int32 off, Int32 len)
            {
                if (stream == null)
                {
                    throw new TTransportException(TTransportException.ExceptionType.NotOpen);
                }

                if (asyncMode)
                {
                    Exception eOuter = null;
                    var evt = new ManualResetEvent(false);
                    var retval = 0;

                    stream.BeginRead(buf, off, len, asyncResult =>
                    {
                        try
                        {
                            if (stream != null)
                                retval = stream.EndRead(asyncResult);
                            else
                                eOuter = new TTransportException(TTransportException.ExceptionType.Interrupted);
                        }
                        catch (Exception e)
                        {
                            if (stream != null)
                                eOuter = e;
                            else
                                eOuter = new TTransportException(TTransportException.ExceptionType.Interrupted, e.Message, e);
                        }
                        evt.Set();
                    }, null);

                    evt.WaitOne();

                    if (eOuter != null)
                        throw eOuter; // rethrow exception
                    else
                        return retval;
                }
                else
                {
                    return stream.Read(buf, off, len);
                }
            }

            public override void Write(Byte[] buf, Int32 off, Int32 len)
            {
                if (stream == null)
                {
                    throw new TTransportException(TTransportException.ExceptionType.NotOpen);
                }

                // if necessary, send the data in chunks
                // there's a system limit around 0x10000 bytes that we hit otherwise
                // MSDN: "Pipe write operations across a network are limited to 65,535 bytes per write. For more information regarding pipes, see the Remarks section."
                var nBytes = Math.Min(len, 15 * 4096);  // 16 would exceed the limit
                while (nBytes > 0)
                {

                    if (asyncMode)
                    {
                        Exception eOuter = null;
                        var evt = new ManualResetEvent(false);

                        stream.BeginWrite(buf, off, nBytes, asyncResult =>
                        {
                            try
                            {
                                if (stream != null)
                                    stream.EndWrite(asyncResult);
                                else
                                    eOuter = new TTransportException(TTransportException.ExceptionType.Interrupted);
                            }
                            catch (Exception e)
                            {
                                if (stream != null)
                                    eOuter = e;
                                else
                                    eOuter = new TTransportException(TTransportException.ExceptionType.Interrupted, e.Message, e);
                            }
                            evt.Set();
                        }, null);

                        evt.WaitOne();

                        if (eOuter != null)
                            throw eOuter; // rethrow exception
                    }
                    else
                    {
                        stream.Write(buf, off, nBytes);
                    }

                    off += nBytes;
                    len -= nBytes;
                    nBytes = Math.Min(len, nBytes);
                }
            }

            protected override void Dispose(Boolean disposing)
            {
                base.Dispose(disposing);

                stream?.Dispose();
            }
        }
    }
#endif
}