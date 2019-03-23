using System;

namespace NewLife.Thrift.Protocol
{
    public struct TStruct
    {
        public TStruct(String name)
            : this()
        {
            Name = name;
        }

        public String Name { get; set; }
    }
}