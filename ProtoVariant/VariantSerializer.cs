using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace ProtoVariant
{
    partial class Variant
    {
        private static Dictionary<Type, VariantSerializer> _serializers = new Dictionary<Type,VariantSerializer>
        {
            { typeof(bool), new BoolSerializer() },
            { typeof(string), new StringSeriaizer() },
            { typeof(int), new Int32Serializer() },
            { typeof(long), new Int64Serializer() },
            { typeof(float), new FloatSerializer() },
            { typeof(double), new DoubleSerializer() },
            { typeof(decimal), new DecimalSerializer() },
            { typeof(DateTime), new DateTimeSerializer() },
            { typeof(byte[]), new BytesSerializer() }
        };

        private abstract class VariantSerializer
        {
            public abstract Variant Serialize(object value);
        }

        private abstract class VariantSerializer<T> : VariantSerializer
        {
            public override Variant Serialize(object value)
            {
                return SerializeCore((T)value);
            }

            protected abstract Variant SerializeCore(T value);
        }

        private class BoolSerializer : VariantSerializer<bool>
        {
            protected override Variant SerializeCore(bool value)
            {
                return new Variant
                {
                    _type = value ? VariantType.BoolTrue : VariantType.BoolFalse
                };
            }
        }

        private class StringSeriaizer : VariantSerializer<string>
        {
            protected override Variant SerializeCore(string value)
            {
                var result = new Variant();

                if (String.Empty.Equals(value))
                    result._type = VariantType.StringEmpty;
                else
                    result._valueString = value;

                return result;
            }
        }

        private class Int32Serializer : VariantSerializer<int>
        {
            protected override Variant SerializeCore(int value)
            {
                var result = new Variant();

                if (value == 0)
                    result._type = VariantType.Int32Zero;
                else
                    result._valueInt32 = value;

                return result;
            }
        }

        private class Int64Serializer : VariantSerializer<long>
        {
            protected override Variant SerializeCore(long value)
            {
                var result = new Variant();

                if (value == 0)
                    result._type = VariantType.Int64Zero;
                else
                    result._valueInt64 = value;

                return result;
            }
        }

        private class FloatSerializer : VariantSerializer<float>
        {
            protected override Variant SerializeCore(float value)
            {
                var result = new Variant();

                if (value == 0f)
                    result._type = VariantType.FloatZero;
                else
                    result._valueFloat = value;

                return result;
            }
        }

        private class DoubleSerializer : VariantSerializer<double>
        {
            protected override Variant SerializeCore(double value)
            {
                var result = new Variant();

                if (value == 0d)
                    result._type = VariantType.DoubleZero;
                else
                    result._valueDouble = value;

                return result;
            }
        }

        private class DecimalSerializer : VariantSerializer<decimal>
        {
            protected override Variant SerializeCore(decimal value)
            {
                return new Variant { _valueDecimal = value.ToString(CultureInfo.InvariantCulture) };
            }
        }

        private class DateTimeSerializer : VariantSerializer<DateTime>
        {
            protected override Variant SerializeCore(DateTime value)
            {
                return new Variant { _valueDateTime = value.ToString("s") };
            }
        }

        private class BytesSerializer : VariantSerializer<byte[]>
        {
            protected override Variant SerializeCore(byte[] value)
            {
                return new Variant { _valueBytes = value };
            }
        }
    }
}
