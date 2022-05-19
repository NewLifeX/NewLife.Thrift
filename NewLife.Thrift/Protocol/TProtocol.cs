using System.Text;
using NewLife.Thrift.Transport;

namespace NewLife.Thrift.Protocol
{
    /// <summary>协议基类</summary>
    public abstract class TProtocol : DisposeBase
    {
        private const Int32 DEFAULT_RECURSION_DEPTH = 64;
        private Int32 recursionDepth;

        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="trans"></param>
        protected TProtocol(TTransport trans)
        {
            Transport = trans;
            RecursionLimit = DEFAULT_RECURSION_DEPTH;
            recursionDepth = 0;
        }

        /// <summary>
        /// 传输口
        /// </summary>
        public TTransport Transport { get; private set; }

        /// <summary>
        /// 递归引用限制
        /// </summary>
        public Int32 RecursionLimit { get; set; }

        /// <summary>
        /// 增加递归引用深度
        /// </summary>
        /// <exception cref="TProtocolException"></exception>
        public void IncrementRecursionDepth()
        {
            if (recursionDepth < RecursionLimit)
                ++recursionDepth;
            else
                throw new TProtocolException(TProtocolException.DEPTH_LIMIT, "Depth limit exceeded");
        }

        /// <summary>
        /// 减少递归引用深度
        /// </summary>
        public void DecrementRecursionDepth() => --recursionDepth;

        public abstract void WriteMessageBegin(TMessage message);
        public abstract void WriteMessageEnd();
        public abstract void WriteStructBegin(TStruct struc);
        public abstract void WriteStructEnd();
        public abstract void WriteFieldBegin(TField field);
        public abstract void WriteFieldEnd();
        public abstract void WriteFieldStop();
        public abstract void WriteMapBegin(TMap map);
        public abstract void WriteMapEnd();
        public abstract void WriteListBegin(TList list);
        public abstract void WriteListEnd();
        public abstract void WriteSetBegin(TSet set);
        public abstract void WriteSetEnd();
        public abstract void WriteBool(Boolean b);
        public abstract void WriteByte(SByte b);
        public abstract void WriteI16(Int16 i16);
        public abstract void WriteI32(Int32 i32);
        public abstract void WriteI64(Int64 i64);
        public abstract void WriteDouble(Double d);
        public virtual void WriteString(String s) => WriteBinary(Encoding.UTF8.GetBytes(s));
        public abstract void WriteBinary(Byte[] b);

        public abstract TMessage ReadMessageBegin();
        public abstract void ReadMessageEnd();
        public abstract TStruct ReadStructBegin();
        public abstract void ReadStructEnd();
        public abstract TField ReadFieldBegin();
        public abstract void ReadFieldEnd();
        public abstract TMap ReadMapBegin();
        public abstract void ReadMapEnd();
        public abstract TList ReadListBegin();
        public abstract void ReadListEnd();
        public abstract TSet ReadSetBegin();
        public abstract void ReadSetEnd();
        public abstract Boolean ReadBool();
        public abstract SByte ReadByte();
        public abstract Int16 ReadI16();
        public abstract Int32 ReadI32();
        public abstract Int64 ReadI64();
        public abstract Double ReadDouble();
        public virtual String ReadString()
        {
            var buf = ReadBinary();
            return Encoding.UTF8.GetString(buf, 0, buf.Length);
        }
        public abstract Byte[] ReadBinary();
    }
}