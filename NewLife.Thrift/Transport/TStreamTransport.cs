using System;
using System.IO;

namespace NewLife.Thrift.Transport
{
    public class TStreamTransport : TTransport
    {
        protected Stream inputStream;
        protected Stream outputStream;

        protected TStreamTransport()
        {
        }

        public TStreamTransport(Stream inputStream, Stream outputStream)
        {
            this.inputStream = inputStream;
            this.outputStream = outputStream;
        }

        public Stream OutputStream => outputStream;

        public Stream InputStream => inputStream;

        public override Boolean IsOpen => true;

        public override void Open()
        {
        }

        public override void Close()
        {
            if (inputStream != null)
            {
                inputStream.Close();
                inputStream = null;
            }
            if (outputStream != null)
            {
                outputStream.Close();
                outputStream = null;
            }
        }

        public override Int32 Read(Byte[] buf, Int32 off, Int32 len)
        {
            if (inputStream == null)
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen, "Cannot read from null inputstream");
            }

            return inputStream.Read(buf, off, len);
        }

        public override void Write(Byte[] buf, Int32 off, Int32 len)
        {
            if (outputStream == null)
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen, "Cannot write to null outputstream");
            }

            outputStream.Write(buf, off, len);
        }

        public override void Flush()
        {
            if (outputStream == null)
            {
                throw new TTransportException(TTransportException.ExceptionType.NotOpen, "Cannot flush null outputstream");
            }

            outputStream.Flush();
        }


        #region 销毁
        private Boolean _IsDisposed;

        /// <summary>销毁</summary>
        protected override void Dispose(Boolean disposing)
        {
            base.Dispose(disposing);

            if (_IsDisposed) return;
            _IsDisposed = true;

            if (disposing)
            {
                InputStream?.Dispose();
                OutputStream?.Dispose();
            }
        }
        #endregion
    }
}
