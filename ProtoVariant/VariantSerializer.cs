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
            public abstract void Serialize(Variant variant, object value);
        }

        private abstract class VariantSerializer<T> : VariantSerializer
        {
            public override void Serialize(Variant variant, object value)
            {
                SerializeCore(variant, (T)value);
            }

            protected abstract void SerializeCore(Variant variant, T value);
        }

        private class BoolSerializer : VariantSerializer<bool>
        {
            protected override void SerializeCore(Variant variant, bool value)
            {
                variant._type = value ? VariantType.BoolTrue : VariantType.BoolFalse;
            }
        }

        private class StringSeriaizer : VariantSerializer<string>
        {
            protected override void SerializeCore(Variant variant, string value)
            {
                if (String.Empty.Equals(value))
                    variant._type = VariantType.StringEmpty;
                else
                    variant._valueString = value;
            }
        }

        private class Int32Serializer : VariantSerializer<int>
        {
            protected override void SerializeCore(Variant variant, int value)
            {
                if (value == 0)
                    variant._type = VariantType.Int32Zero;
                else
                    variant._valueInt32 = value;
            }
        }

        private class Int64Serializer : VariantSerializer<long>
        {
            protected override void SerializeCore(Variant variant, long value)
            {
                if (value == 0)
                    variant._type = VariantType.Int64Zero;
                else
                    variant._valueInt64 = value;
            }
        }

        private class FloatSerializer : VariantSerializer<float>
        {
            protected override void SerializeCore(Variant variant, float value)
            {
                if (value == 0f)
                    variant._type = VariantType.FloatZero;
                else
                    variant._valueFloat = value;
            }
        }

        private class DoubleSerializer : VariantSerializer<double>
        {
            protected override void SerializeCore(Variant variant, double value)
            {
                if (value == 0d)
                    variant._type = VariantType.DoubleZero;
                else
                    variant._valueDouble = value;
            }
        }

        private class DecimalSerializer : VariantSerializer<decimal>
        {
            protected override void SerializeCore(Variant variant, decimal value)
            {
                variant._valueDecimal = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        private class DateTimeSerializer : VariantSerializer<DateTime>
        {
            protected override void SerializeCore(Variant variant, DateTime value)
            {
                long ticks = value.Ticks;

                if (ticks == 0L)
                    variant._type = VariantType.DateTimeZero;
                else
                    variant._valueDateTime = ticks;
            }
        }

        private class BytesSerializer : VariantSerializer<byte[]>
        {
            protected override void SerializeCore(Variant variant, byte[] value)
            {
                variant._valueBytes = value;
            }
        }
    }
}
