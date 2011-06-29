using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace ProtoVariant
{
    [ProtoContract]
    internal enum VariantType
    {
        Auto,
        BoolFalse,
        BoolTrue,
        Int32Zero,
        Int64Zero,
        FloatZero,
        DoubleZero,
        StringEmpty,
        Null
    }
}
