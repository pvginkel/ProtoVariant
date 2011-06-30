using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using System.Globalization;

namespace ProtoVariant
{
    [ProtoContract]
    public partial class Variant
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

        [ProtoMember(4, IsRequired = false, DataFormat = DataFormat.FixedSize)]
        private float _valueFloat;

        [ProtoMember(5, IsRequired = false, DataFormat = DataFormat.TwosComplement)]
        private double _valueDouble = default(double);

        [ProtoMember(6, IsRequired = false, DataFormat = DataFormat.Default)]
        private string _valueString;

        [ProtoMember(7, IsRequired = false, DataFormat = DataFormat.Default)]
        private byte[] _valueBytes;

        [ProtoMember(8, IsRequired = false, DataFormat = DataFormat.Default)]
        private string _valueDecimal;

        [ProtoMember(9, IsRequired = false, DataFormat = DataFormat.Default)]
        private string _valueDateTime;

        private Variant()
        {
        }

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

        private object ConstructValue()
        {
            switch (_type)
            {
                case VariantType.BoolFalse: return false;
                case VariantType.BoolTrue: return true;
                case VariantType.DoubleZero: return 0d;
                case VariantType.FloatZero: return 0f;
                case VariantType.Int32Zero: return 0;
                case VariantType.Int64Zero: return 0L;
                case VariantType.Null: return null;
                case VariantType.StringEmpty: return String.Empty;

                default:
                    if (_valueInt32 != 0)
                        return _valueInt32;
                    if (_valueInt64 != 0L)
                        return _valueInt64;
                    if (_valueDouble != 0d)
                        return _valueDouble;
                    if (_valueFloat != 0f)
                        return _valueFloat;
                    if (!String.IsNullOrEmpty(_valueString))
                        return _valueString;
                    if (_valueBytes != null)
                        return _valueBytes;
                    if (!String.IsNullOrEmpty(_valueDateTime))
                        return DateTime.ParseExact(_valueDateTime, "s", CultureInfo.InvariantCulture);
                    if (!String.IsNullOrEmpty(_valueDecimal))
                        return Decimal.Parse(_valueDecimal, CultureInfo.InvariantCulture);
                    return null;
            }
        }
    }
}
