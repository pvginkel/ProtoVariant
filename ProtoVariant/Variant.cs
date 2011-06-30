using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using System.Globalization;

namespace ProtoVariant
{
    /// <summary>
    /// Represents a variant message used with protobuf. This message can contain
    /// a value of type int, long, float, double, string, byte[], decimal or
    /// DateTime, or can be null, while guarenteeing lossless roundtrips.
    /// </summary>
    [ProtoContract]
    public sealed partial class Variant
    {
        // With protobuf-net we have the option of making the struct properties
        // nullable (e.g. int?). This way we can control whether optional
        // properties are serialized and actually serialize 0 for int32 even
        // though it's optional. The reason we're not doing that is to keep
        // the size of Variant down. If we would implement this, we would
        // require 20 extra bytes with little added value (the boolean values
        // of the Nullable<T> struct). Also, this depends on implementation
        // details of protobuf-net and I feel this lessens compatability.

        private bool _haveValue;
        private object _value;

        [ProtoMember(1, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        private VariantType _type;

        [ProtoMember(2, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        private int _valueInt32;

        [ProtoMember(3, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        private long _valueInt64;

        [ProtoMember(4, IsRequired = false)]
        private float _valueFloat;

        [ProtoMember(5, IsRequired = false)]
        private double _valueDouble;

        [ProtoMember(6, IsRequired = false)]
        private string _valueString;

        [ProtoMember(7, IsRequired = false)]
        private byte[] _valueBytes;

        // Decimal is stored as a string because this is the only interoperably
        // way to guarentee lossless transfer of data. .NET has the option of
        // serializing Decimals using their raw data, but this always takes 16
        // bytes and is less interoperable.
        [ProtoMember(8, IsRequired = false)]
        private string _valueDecimal;

        // DateTime's are stored as FixedSize because it generally contains
        // larges values. As of this moment, using TwosComplement already takes
        // up one extra byte when serializing DateTime.Now.
        [ProtoMember(9, IsRequired = false, DataFormat = DataFormat.FixedSize)]
        private long _valueDateTime;

        private Variant()
        {
            // Private constructor for deserialization.
        }

        /// <summary>
        /// Initialize a new <see cref="Variant"/> from an existing value.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not
        /// of a type which can be safely stored in a <see cref="Variant"/>.</exception>
        /// <param name="value">Value to be stored in the <see cref="Variant"/>.</param>
        public Variant(object value)
        {
            _haveValue = true;
            _value = value;

            if (value == null)
            {
                _type = VariantType.Null;
            }
            else
            {
                VariantSerializer serializer;

                if (!_serializers.TryGetValue(value.GetType(), out serializer))
                    throw new ArgumentException(String.Format("Cannot serialize type {0} to Variant", value.GetType().FullName), "value");

                serializer.Serialize(this, value);
            }
        }

        /// <summary>
        /// Gets the value stored in the <see cref="Variant"/>.
        /// </summary>
        public object Value
        {
            get
            {
                if (!_haveValue)
                {
                    _value = ConstructValue();
                    _haveValue = true;
                }

                return _value;
            }
        }
    }
}
