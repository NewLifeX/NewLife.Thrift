namespace NewLife.Thrift.Transport;

/// <summary>
/// 传输基类
/// </summary>
public abstract class TTransport : DisposeBase
{
    /// <summary>
    /// 已打开
    /// </summary>
    public abstract Boolean IsOpen { get; }

    private readonly Byte[] _peekBuffer = new Byte[1];
    private Boolean _hasPeekByte;

    /// <summary>
    /// 读取一个字节，返回是否成功
    /// </summary>
    /// <returns></returns>
    public Boolean Peek()
    {
        if (_hasPeekByte) return true;
        if (!IsOpen) return false;

        try
        {
            var bytes = Read(_peekBuffer, 0, 1);
            if (bytes == 0) return false;
        }
        catch (IOException)
        {
            return false;
        }

        _hasPeekByte = true;
        return true;
    }

    /// <summary>
    /// 打开
    /// </summary>
    public abstract void Open();

    /// <summary>
    /// 关闭
    /// </summary>
    public abstract void Close();

    /// <summary>
    /// 验证缓冲区参数
    /// </summary>
    /// <param name="buf"></param>
    /// <param name="off"></param>
    /// <param name="len"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    protected static void ValidateBufferArgs(Byte[] buf, Int32 off, Int32 len)
    {
        if (buf == null)
            throw new ArgumentNullException("buf");
        if (off < 0)
            throw new ArgumentOutOfRangeException("Buffer offset is smaller than zero.");
        if (len < 0)
            throw new ArgumentOutOfRangeException("Buffer length is smaller than zero.");
        if (off + len > buf.Length)
            throw new ArgumentOutOfRangeException("Not enough data.");
    }

    /// <summary>
    /// 读取
    /// </summary>
    /// <param name="buf"></param>
    /// <param name="off"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    public abstract Int32 Read(Byte[] buf, Int32 off, Int32 len);

    /// <summary>
    /// 读取所有
    /// </summary>
    /// <param name="buf"></param>
    /// <param name="off"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    /// <exception cref="TTransportException"></exception>
    public Int32 ReadAll(Byte[] buf, Int32 off, Int32 len)
    {
        ValidateBufferArgs(buf, off, len);
        var got = 0;

        //If we previously peeked a byte, we need to use that first.
        if (_hasPeekByte)
        {
            buf[off + got++] = _peekBuffer[0];
            _hasPeekByte = false;
        }

        while (got < len)
        {
            var ret = Read(buf, off + got, len - got);
            if (ret <= 0)
            {
                throw new TTransportException(
                    TTransportException.ExceptionType.EndOfFile,
                    "Cannot read, Remote side has closed");
            }
            got += ret;
        }
        return got;
    }

    /// <summary>
    /// 写入
    /// </summary>
    /// <param name="buf"></param>
    public virtual void Write(Byte[] buf) => Write(buf, 0, buf.Length);

    /// <summary>
    /// 写入
    /// </summary>
    /// <param name="buf"></param>
    /// <param name="off"></param>
    /// <param name="len"></param>
    public abstract void Write(Byte[] buf, Int32 off, Int32 len);

    /// <summary>
    /// 刷新写入缓冲区，让数据发出或落盘
    /// </summary>
    public virtual void Flush() { }

    /// <summary>
    /// 开始刷缓冲区
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="TTransportException"></exception>
    public virtual IAsyncResult BeginFlush(AsyncCallback callback, Object state)
    {
        throw new TTransportException(
            TTransportException.ExceptionType.Unknown,
            "Asynchronous operations are not supported by this transport.");
    }

    /// <summary>
    /// 结束刷缓冲区
    /// </summary>
    /// <param name="asyncResult"></param>
    /// <exception cref="TTransportException"></exception>
    public virtual void EndFlush(IAsyncResult asyncResult)
    {
        throw new TTransportException(
            TTransportException.ExceptionType.Unknown,
            "Asynchronous operations are not supported by this transport.");
    }
}
