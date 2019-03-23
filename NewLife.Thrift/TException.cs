using System;

namespace NewLife.Thrift
{
    public class TException : Exception
    {
        public TException()
        {
        }

        public TException(String message, Exception inner = null)
            : base(message, inner)
        {
        }
    }
}