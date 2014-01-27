using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoVariant
{
    public interface IVariantValueProvider
    {
        int SubType { get; }
        Type Type { get; }
        string Encode(object value);
        object Decode(string value);
    }
}
