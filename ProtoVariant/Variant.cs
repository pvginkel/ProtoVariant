using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using ProtoBuf;

namespace ProtoVariant
{
    [ProtoContract]
    [Serializable]
    [DebuggerDisplay("Value={Value}, Type={Type}")]
    public sealed class Variant : IEquatable<Variant>
    {
        private static readonly object _syncRoot = new object();

        private static readonly Dictionary<VariantType, object> FixedFromType = new Dictionary<VariantType, object>
        {
            { VariantType.Null, null },
            { VariantType.EmptyString, String.Empty },
            { VariantType.False, false },
            { VariantType.True, true },
            { VariantType.Int32Zero, 0 },
            { VariantType.Int32One, 1 },
            { VariantType.Int64Zero, (long)0 },
            { VariantType.SingleZero, 0f },
            { VariantType.DoubleZero, 0d },
            { VariantType.DecimalZero, 0m },
            { VariantType.EmptyColor, Color.Empty }
        };

        private static readonly Dictionary<VariantType, IValueProvider> ProviderFromVariantType = new Dictionary<VariantType, IValueProvider>
        {
            { VariantType.Int32, new ValueProvider<int>(VariantType.Int32) },
            { VariantType.Int64, new ValueProvider<long>(VariantType.Int64) },
            { VariantType.Single, new ValueProvider<float>(VariantType.Single) },
            { VariantType.Double, new ValueProvider<double>(VariantType.Double) },
            { VariantType.String, new ValueProvider<string>(VariantType.String) },
            { VariantType.Decimal, new ValueProvider<decimal>(VariantType.Decimal) },
            { VariantType.DateTime, new DateTimeValueProvider() },
            { VariantType.Color, new ColorValueProvider() },
        };

        private static readonly Dictionary<object, VariantType> FixedFromValue = BuildFixedFromValue();

        private static Dictionary<object, VariantType> BuildFixedFromValue()
        {
            var result = new Dictionary<object, VariantType>();

            foreach (var item in FixedFromType)
            {
                if (item.Value != null)
                    result.Add(item.Value, item.Key);
            }

            return result;
        }

        private static readonly Dictionary<Type, IValueProvider> ProviderFromType = BuildProviderFromType();

        private static Dictionary<Type, IValueProvider> BuildProviderFromType()
        {
            // We want to re-use the value providers, so we build this from
            // the _providerFromVariantType.

            var result = new Dictionary<Type, IValueProvider>();

            foreach (var provider in ProviderFromVariantType.Values)
            {
                result.Add(provider.Type, provider);
            }

            return result;
        }

        private static Dictionary<int, IVariantValueProvider> _customValueProviderBySubType = new Dictionary<int, IVariantValueProvider>();
        private static Dictionary<Type, IVariantValueProvider> _customValueProviderByType = new Dictionary<Type, IVariantValueProvider>();

        public static void RegisterValueProvider(IVariantValueProvider valueProvider)
        {
            if (valueProvider == null)
                throw new ArgumentNullException("valueProvider");

            lock (_syncRoot)
            {
                _customValueProviderBySubType = new Dictionary<int, IVariantValueProvider>(_customValueProviderBySubType)
                {
                    { valueProvider.SubType, valueProvider }
                };

                _customValueProviderByType = new Dictionary<Type, IVariantValueProvider>(_customValueProviderByType)
                {
                    { valueProvider.Type, valueProvider }
                };
            }
        }

        [ProtoMember(1)]
        [DefaultValue(VariantType.Null)]
        internal VariantType Type { get; set; }

        [ProtoMember(2, IsRequired = false)]
        [DefaultValue(null)]
        internal string StringValue { get; set; }

        [ProtoMember(3, IsRequired = false)]
        [DefaultValue(0)]
        internal int IntValue { get; set; }

        public object Value
        {
            get
            {
                object result;

                if (FixedFromType.TryGetValue(Type, out result))
                    return result;

                if (Type == VariantType.Custom)
                {
                    IVariantValueProvider customProvider;

                    if (!_customValueProviderBySubType.TryGetValue(IntValue, out customProvider))
                        throw new InvalidOperationException("No value provider registered");

                    return customProvider.Decode(StringValue);
                }

                if (Type == VariantType.Int32)
                    return IntValue;

                Debug.Assert(StringValue != null);

                if (Type == VariantType.String)
                    return StringValue;

                IValueProvider provider;

                if (!ProviderFromVariantType.TryGetValue(Type, out provider))
                    throw new InvalidOperationException("Cannot decode");

                return provider.Decode(StringValue);
            }
            set
            {
                StringValue = null;

                if (value == null)
                {
                    Type = VariantType.Null;
                }
                else
                {
                    VariantType type;

                    if (FixedFromValue.TryGetValue(value, out type))
                    {
                        Type = type;
                    }
                    else
                    {
                        var valueType = value.GetType();
                        IVariantValueProvider customProvider;
                        IValueProvider provider;

                        if (_customValueProviderByType.TryGetValue(valueType, out customProvider))
                        {
                            Type = VariantType.Custom;
                            IntValue = customProvider.SubType;
                            StringValue = customProvider.Encode(value);
                        }
                        else if (ProviderFromType.TryGetValue(valueType, out provider))
                        {
                            Type = provider.VariantType;

                            if (Type == VariantType.Int32)
                            {
                                IntValue = (int)value;
                                StringValue = null;
                            }
                            else if (Type == VariantType.String)
                            {
                                StringValue = (string)value;
                                IntValue = 0;
                            }
                            else
                            {
                                StringValue = provider.Encode(value);
                                IntValue = 0;
                            }
                        }
                        else
                        {
                            throw new ArgumentException("Cannot encode", "value");
                        }
                    }
                }
            }
        }

        public Variant()
        {
        }

        public Variant(object value)
        {
            Value = value;
        }

        public bool IsNull
        {
            get { return Type == VariantType.Null; }
        }

        public bool ToBool()
        {
            return (bool)Value;
        }

        public int ToInt32()
        {
            return (int)Value;
        }

        public long ToInt64()
        {
            object value = Value;

            if (value is int)
                return (int)value;

            return (long)value;
        }

        public override string ToString()
        {
            return (string)Value;
        }

        public float ToSingle()
        {
            return (float)Value;
        }

        public double ToDouble()
        {
            object value = Value;

            if (value is float)
                return (float)value;

            return (double)value;
        }

        public decimal ToDecimal()
        {
            return (decimal)Value;
        }

        public DateTime ToDateTime()
        {
            return (DateTime)Value;
        }

        public Color ToColor()
        {
            return (Color)Value;
        }

        public override int GetHashCode()
        {
            object value = Value;

            return value == null ? 0 : value.GetHashCode();
        }

        public bool Equals(Variant other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return Equals(Value, other.Value);
        }

        public static implicit operator bool(Variant variant)
        {
            return variant.ToBool();
        }

        public static implicit operator Variant(bool value)
        {
            return new Variant(value);
        }

        public static implicit operator int(Variant variant)
        {
            return variant.ToInt32();
        }

        public static implicit operator Variant(int value)
        {
            return new Variant(value);
        }

        public static implicit operator long(Variant variant)
        {
            return variant.ToInt64();
        }

        public static implicit operator Variant(long value)
        {
            return new Variant(value);
        }

        public static implicit operator string(Variant variant)
        {
            return variant.ToString();
        }

        public static implicit operator Variant(string value)
        {
            return new Variant(value);
        }

        public static implicit operator float(Variant variant)
        {
            return variant.ToSingle();
        }

        public static implicit operator Variant(float value)
        {
            return new Variant(value);
        }

        public static implicit operator double(Variant variant)
        {
            return variant.ToDouble();
        }

        public static implicit operator Variant(double value)
        {
            return new Variant(value);
        }

        public static implicit operator decimal(Variant variant)
        {
            return variant.ToDecimal();
        }

        public static implicit operator Variant(decimal value)
        {
            return new Variant(value);
        }

        public static implicit operator DateTime(Variant variant)
        {
            return variant.ToDateTime();
        }

        public static implicit operator Color(Variant variant)
        {
            return variant.ToColor();
        }

        public static implicit operator Variant(DateTime value)
        {
            return new Variant(value);
        }

        private interface IValueProvider
        {
            Type Type { get; }
            VariantType VariantType { get; }
            string Encode(object value);
            object Decode(string value);
        }

        private class ValueProvider<T> : IValueProvider
        {
            private readonly XsdType _type = XsdType.FindConverter(typeof(T));

            public Type Type
            {
                get { return typeof(T); }
            }

            public VariantType VariantType { get; private set; }

            public ValueProvider(VariantType variantType)
            {
                VariantType = variantType;
            }

            public string Encode(object value)
            {
                return _type.Encode(value);
            }

            public object Decode(string value)
            {
                return _type.Decode(value);
            }
        }

        private class DateTimeValueProvider : IValueProvider
        {
            public Type Type
            {
                get { return typeof(DateTime); }
            }

            public VariantType VariantType
            {
                get { return VariantType.DateTime; }
            }

            public string Encode(object value)
            {
                var dateTime = (DateTime)value;

                return String.Format(
                    "{0}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}",
                    dateTime.Year,
                    dateTime.Month,
                    dateTime.Day,
                    dateTime.Hour,
                    dateTime.Minute,
                    dateTime.Second
                );
            }

            public object Decode(string value)
            {
                int offset = 0;

                return new DateTime(
                    Eat(value, ref offset, '-'),
                    Eat(value, ref offset, '-'),
                    Eat(value, ref offset, 'T'),
                    Eat(value, ref offset, ':'),
                    Eat(value, ref offset, ':'),
                    Eat(value, ref offset, -1)
                );
            }

            private int Eat(string value, ref int offset, int c)
            {
                string part;

                if (c == -1)
                {
                    part = value.Substring(offset);
                }
                else
                {
                    int start = offset;

                    while (offset < value.Length && value[offset] != c)
                    {
                        offset++;
                    }

                    if (offset == value.Length)
                        throw new InvalidOperationException("Cannot decode date/time");

                    part = value.Substring(start, offset - start);
                    offset++;
                }

                return int.Parse(part, NumberStyles.None, CultureInfo.InvariantCulture);
            }
        }

        private class ColorValueProvider : IValueProvider
        {
            public Type Type
            {
                get { return typeof(Color); }
            }

            public VariantType VariantType
            {
                get { return VariantType.Color; }
            }

            public string Encode(object value)
            {
                var color = (Color)value;

                return
                    color.A == 255
                    ? String.Format("{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B)
                    : String.Format("{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
            }

            public object Decode(string value)
            {
                if (value.Length == 6)
                {
                    return Color.FromArgb(
                        int.Parse(value.Substring(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture),
                        int.Parse(value.Substring(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture),
                        int.Parse(value.Substring(4, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)
                    );
                }
                else
                {
                    Debug.Assert(value.Length == 8);

                    return Color.FromArgb(
                        int.Parse(value.Substring(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture),
                        int.Parse(value.Substring(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture),
                        int.Parse(value.Substring(4, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture),
                        int.Parse(value.Substring(6, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture)
                    );
                }
            }
        }
    }
}
