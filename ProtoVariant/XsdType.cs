using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;

namespace ProtoVariant
{
    internal abstract class XsdType
    {
        private static readonly object _syncRoot = new object();
        private static readonly Dictionary<Type, EnumConverter> EnumConverters = new Dictionary<Type, EnumConverter>();

        public const string Bool = "xs:boolean";
        public const string Byte = "xs:unsignedByte";
        public const string ByteArray = "xs:base64Binary";
        public const string Char = "char";
        public const string Date = "xs:date";
        public const string DateTime = "xs:dateTime";
        public const string Decimal = "xs:decimal";
        public const string Double = "xs:double";
        public const string Enum = "enum";
        public const string Guid = "guid";
        public const string Int16 = "xs:short";
        public const string Int32 = "xs:int";
        public const string Int64 = "xs:long";
        public const string SByte = "xs:byte";
        public const string Single = "xs:float";
        public const string String = "xs:string";
        public const string UInt16 = "xs:unsignedShort";
        public const string UInt32 = "xs:unsignedInt";
        public const string UInt64 = "xs:unsignedLong";
        public const string TimeSpan = "xs:duration";

        private static readonly Dictionary<Type, XsdType> _convertersByType = new Dictionary<Type, XsdType>();
        private static readonly Dictionary<string, XsdType> _convertersByName = new Dictionary<string, XsdType>();

        static XsdType()
        {
            foreach (var converter in new XsdType[] {
                new GuidConverter(),
                new SByteConverter(),
                new ByteConverter(),
                new ShortConverter(),
                new UShortConverter(),
                new IntConverter(),
                new UIntConverter(),
                new LongConverter(),
                new ULongConverter(),
                new DecimalConverter(),
                new FloatConverter(),
                new DoubleConverter(),
                new BoolConverter(),
                new CharConverter(),
                new StringConverter(),
                new ByteArrayConverter(),
                new DateConverter(),
                new DateTimeConverter(),
                new TimeSpanConverter()
            })
            {
                if (converter.IsDefault)
                    _convertersByType.Add(converter.Type, converter);

                _convertersByName.Add(converter.SchemaType, converter);
            }
        }

        public static XsdType FindConverter(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var actualType = Nullable.GetUnderlyingType(type) ?? type;
            bool isNullable = !type.IsInterface && !type.IsClass && IsNullable(type);

            XsdType result;

            if (actualType.IsEnum)
            {
                lock (_syncRoot)
                {
                    EnumConverter enumConverter;

                    if (!EnumConverters.TryGetValue(actualType, out enumConverter))
                    {
                        enumConverter = new EnumConverter(actualType);

                        EnumConverters.Add(actualType, enumConverter);
                    }

                    result = enumConverter;
                }
            }
            else
            {
                _convertersByType.TryGetValue(actualType, out result);
            }

            if (result != null && isNullable)
                result = new NullableConverter(actualType, result);

            return result;
        }

        private static bool IsNullable(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return
                type.IsClass ||
                type.IsInterface || (
                    type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(Nullable<>)
                );
        }

        public static XsdType FindConverter(string schemaType)
        {
            if (schemaType == null)
                throw new ArgumentNullException("schemaType");

            XsdType result;

            _convertersByName.TryGetValue(schemaType, out result);

            return result;
        }

        public abstract string SchemaType { get; }
        public abstract Type Type { get; }

        protected virtual bool IsDefault
        {
            get { return true; }
        }

        public abstract string Encode(object value);
        public abstract object Decode(string value);

        private class NullableConverter : XsdType
        {
            private readonly Type _type;
            private readonly XsdType _baseConverter;

            public NullableConverter(Type type, XsdType baseConverter)
            {
                if (type == null)
                    throw new ArgumentNullException("type");
                if (baseConverter == null)
                    throw new ArgumentNullException("baseConverter");

                _type = type;
                _baseConverter = baseConverter;
            }

            public override Type Type
            {
                get { return _type; }
            }

            public override string SchemaType
            {
                get { return _baseConverter.SchemaType; }
            }

            public override string Encode(object value)
            {
                if (value == null)
                    return null;
                else
                    return _baseConverter.Encode(value);
            }

            public override object Decode(string value)
            {
                if (String.IsNullOrEmpty(value))
                    return null;
                else
                    return _baseConverter.Decode(value);
            }
        }

        private class GuidConverter : XsdType
        {
            public override Type Type { get { return typeof(Guid); } }
            public override string SchemaType { get { return Guid; } }

            public override string Encode(object value)
            {
                if (value == null)
                    return null;
                else
                    return ((Guid)value).ToString();
            }

            public override object Decode(string value)
            {
                if (String.IsNullOrEmpty(value))
                    return System.Guid.Empty;
                else
                    return new Guid(value);
            }
        }

        private class SByteConverter : XsdType
        {
            public override string SchemaType { get { return SByte; } }
            public override Type Type { get { return typeof(sbyte); } }

            public override string Encode(object value)
            {
                return ((sbyte)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                return System.SByte.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private class ByteConverter : XsdType
        {
            public override string SchemaType { get { return Byte; } }
            public override Type Type { get { return typeof(byte); } }

            public override string Encode(object value)
            {
                return ((byte)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                return System.Byte.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private class ShortConverter : XsdType
        {
            public override string SchemaType { get { return Int16; } }
            public override Type Type { get { return typeof(short); } }

            public override string Encode(object value)
            {
                return ((short)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                return System.Int16.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private class UShortConverter : XsdType
        {
            public override string SchemaType { get { return UInt16; } }
            public override Type Type { get { return typeof(ushort); } }

            public override string Encode(object value)
            {
                return ((ushort)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                return System.UInt16.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private class IntConverter : XsdType
        {
            public override string SchemaType { get { return Int32; } }
            public override Type Type { get { return typeof(int); } }

            public override string Encode(object value)
            {
                return ((int)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                return System.Int32.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private class UIntConverter : XsdType
        {
            public override string SchemaType { get { return UInt32; } }
            public override Type Type { get { return typeof(uint); } }

            public override string Encode(object value)
            {
                return ((uint)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                return System.UInt32.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private class LongConverter : XsdType
        {
            public override string SchemaType { get { return Int64; } }
            public override Type Type { get { return typeof(long); } }

            public override string Encode(object value)
            {
                return ((long)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                return System.Int64.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private class ULongConverter : XsdType
        {
            public override string SchemaType { get { return UInt64; } }
            public override Type Type { get { return typeof(ulong); } }

            public override string Encode(object value)
            {
                return ((ulong)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                return System.UInt64.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private class DecimalConverter : XsdType
        {
            public override string SchemaType { get { return Decimal; } }
            public override Type Type { get { return typeof(decimal); } }

            public override string Encode(object value)
            {
                return ((decimal)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                return System.Decimal.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private class FloatConverter : XsdType
        {
            public override string SchemaType { get { return Single; } }
            public override Type Type { get { return typeof(float); } }

            public override string Encode(object value)
            {
                return ((float)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                return System.Single.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private class DoubleConverter : XsdType
        {
            public override string SchemaType { get { return Double; } }
            public override Type Type { get { return typeof(double); } }

            public override string Encode(object value)
            {
                return ((double)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                return System.Double.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private class BoolConverter : XsdType
        {
            public override string SchemaType { get { return Bool; } }
            public override Type Type { get { return typeof(bool); } }

            public override string Encode(object value)
            {
                if ((bool)value)
                    return "true";
                else
                    return "false";
            }

            public override object Decode(string value)
            {
                switch (value)
                {
                    case "true":
                    case "1":
                        return true;

                    case "false":
                    case "0":
                        return false;

                    default:
                        throw new ArgumentException("Cannot decode value", "value");
                }
            }
        }

        private class CharConverter : XsdType
        {
            public override string SchemaType { get { return Char; } }
            public override Type Type { get { return typeof(char); } }

            public override string Encode(object value)
            {
                return ((char)value).ToString(CultureInfo.InvariantCulture);
            }

            public override object Decode(string value)
            {
                if (value == null || value.Length != 1)
                    throw new ArgumentException("Cannot decode value", "value");

                return value[0];
            }
        }

        private class StringConverter : XsdType
        {
            public override string SchemaType { get { return String; } }
            public override Type Type { get { return typeof(string); } }

            public override string Encode(object value)
            {
                return (string)value;
            }

            public override object Decode(string value)
            {
                return value;
            }
        }

        private class ByteArrayConverter : XsdType
        {
            public override string SchemaType { get { return ByteArray; } }
            public override Type Type { get { return typeof(byte[]); } }

            public override string Encode(object value)
            {
                if (value == null)
                    return null;
                else
                    return Convert.ToBase64String((byte[])value);
            }

            public override object Decode(string value)
            {
                if (String.IsNullOrEmpty(value))
                    return null;
                else
                    return Convert.FromBase64String(value);
            }
        }

        private class DateConverter : XsdType
        {
            public override string SchemaType { get { return Date; } }
            public override Type Type { get { return typeof(DateTime); } }
            protected override bool IsDefault { get { return false; } }

            public override string Encode(object value)
            {
                return new SoapDate((DateTime)value).ToString();
            }

            public override object Decode(string value)
            {
                return SoapDate.Parse(value).Value;
            }
        }

        private class DateTimeConverter : XsdType
        {
            public override string SchemaType { get { return DateTime; } }
            public override Type Type { get { return typeof(DateTime); } }

            public override string Encode(object value)
            {
                return SoapDateTime.ToString((DateTime)value);
            }

            public override object Decode(string value)
            {
                return SoapDateTime.Parse(value);
            }
        }

        private class TimeSpanConverter : XsdType
        {
            public override string SchemaType { get { return TimeSpan; } }
            public override Type Type { get { return typeof(TimeSpan); } }

            public override string Encode(object value)
            {
                return SoapDuration.ToString((TimeSpan)value);
            }

            public override object Decode(string value)
            {
                return SoapDuration.Parse(value);
            }
        }

        private class EnumConverter : XsdType
        {
            private readonly Type _type;

            public EnumConverter(Type type)
            {
                if (type == null)
                    throw new ArgumentNullException("type");

                _type = type;
            }

            public override string SchemaType { get { return Enum; } }
            public override Type Type { get { return _type; } }

            public override string Encode(object value)
            {
                return value.ToString();
            }

            public override object Decode(string value)
            {
                return System.Enum.Parse(_type, value);
            }
        }
    }
}
