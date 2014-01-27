using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace ProtoVariant
{
    [ProtoContract]
    public enum VariantType
    {
        Null,
        Int32,
        Int64,
        Single,
        Double,
        String,
        Decimal,
        DateTime,
        EmptyString,
        False,
        True,
        Int32Zero,
        Int32One,
        Int64Zero,
        SingleZero,
        DoubleZero,
        DecimalZero,
        Color,
        EmptyColor,
        Custom
    }
}
