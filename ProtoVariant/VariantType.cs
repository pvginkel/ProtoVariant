using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace ProtoVariant
{
    [ProtoContract]
    internal enum VariantType
    {
        [ProtoEnum(Value = 0)]
        Auto = 0,
        [ProtoEnum(Value = 1)]
        Null = 1,
        [ProtoEnum(Value = 2)]
        BoolFalse = 2,
        [ProtoEnum(Value = 3)]
        BoolTrue = 3,
        [ProtoEnum(Value = 4)]
        Int32Zero = 4,
        [ProtoEnum(Value = 5)]
        Int64Zero = 5,
        [ProtoEnum(Value = 6)]
        FloatZero = 6,
        [ProtoEnum(Value = 7)]
        DoubleZero = 7,
        [ProtoEnum(Value = 8)]
        DateTimeZero = 8,
        [ProtoEnum(Value = 9)]
        StringEmpty = 9
    }
}
