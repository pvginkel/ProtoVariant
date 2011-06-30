using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using System.Globalization;
using System.Collections;

namespace ProtoVariant
{
    /// <summary>
    /// Represents a variant message used with protobuf. This message can contain
    /// a value of type int, long, float, double, string, byte[], decimal or
    /// DateTime, or can be null, while guarenteeing lossless roundtrips.
    /// </summary>
    [ProtoContract]
    [Serializable]
    public sealed class Variant : IEquatable<Variant>
    {
        /// <summary>
        /// Gets the value stored in the <see cref="Variant"/>.
        /// </summary>
        public object Value { get; private set; }

        [ProtoMember(1, IsRequired = true)]
        private bool ValueBool
        {
            get { return (bool)Value; }
            set { Value = (bool)value; }
        }

        private bool ShouldSerializeValueBool()
        {
            return Value is bool;
        }

        [ProtoMember(2, IsRequired = true, DataFormat = DataFormat.TwosComplement)]
        private int ValueInt32
        {
            get { return (int)Value; }
            set { Value = (int)value; }
        }

        private bool ShouldSerializeValueInt32()
        {
            return Value is int;
        }

        [ProtoMember(3, IsRequired = true, DataFormat = DataFormat.TwosComplement)]
        private long ValueInt64
        {
            get { return (long)Value; }
            set { Value = (long)value; }
        }

        private bool ShouldSerializeValueInt64()
        {
            return Value is long;
        }

        [ProtoMember(4, IsRequired = true)]
        private float ValueFloat
        {
            get { return (float)Value; }
            set { Value = (float)value; }
        }

        private bool ShouldSerializeValueFloat()
        {
            return Value is float;
        }

        [ProtoMember(5, IsRequired = true)]
        private double ValueDouble
        {
            get { return (double)Value; }
            set { Value = (double)value; }
        }

        private bool ShouldSerializeValueDouble()
        {
            return Value is double;
        }

        [ProtoMember(6, IsRequired = true)]
        private string ValueString
        {
            get { return (string)Value; }
            set { Value = (string)value; }
        }

        private bool ShouldSerializeValueString()
        {
            return Value is string;
        }

        [ProtoMember(7, IsRequired = true)]
        private byte[] ValueBytes
        {
            get { return (byte[])Value; }
            set { Value = (byte[])value; }
        }

        private bool ShouldSerializeValueBytes()
        {
            return Value is byte[];
        }

        // Decimal is stored as a string because this is the only interoperably
        // way to guarentee lossless transfer of data. .NET has the option of
        // serializing Decimals using their raw data, but this always takes 16
        // bytes and is less interoperable.
        [ProtoMember(8, IsRequired = true)]
        private string ValueDecimal
        {
            get { return ((decimal)Value).ToString(CultureInfo.InvariantCulture); }
            set { Value = Decimal.Parse((string)value, CultureInfo.InvariantCulture); }
        }

        private bool ShouldSerializeValueDecimal()
        {
            return Value is decimal;
        }

        // DateTime's are stored as FixedSize because it generally contains
        // larges values. As of this moment, using TwosComplement already takes
        // up one extra byte when serializing DateTime.Now.
        [ProtoMember(9, IsRequired = true, DataFormat = DataFormat.FixedSize)]
        private long ValueDateTime
        {
            get { return ((DateTime)Value).Ticks; }
            set { Value = new DateTime((long)value); }
        }

        private bool ShouldSerializeValueDateTime()
        {
            return Value is DateTime;
        }

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
            Value = value;
        }

        /// <summary>
        /// Returns true when the contained value is null.
        /// </summary>
        public bool IsNull
        {
            get { return Value == null; }
        }

        /// <summary>
        /// Returns the contained value as a boolean.
        /// </summary>
        /// <returns>Contained value as a boolean.</returns>
        /// <exception cref="InvalidCastException">When the contained value is not a boolean.</exception>
        public bool ToBool()
        {
            return ValueBool;
        }

        /// <summary>
        /// Returns the contained value as an integer.
        /// </summary>
        /// <returns>Contained value as an integer.</returns>
        /// <exception cref="InvalidCastException">When the contained value is not an integer.</exception>
        public int ToInt32()
        {
            return ValueInt32;
        }

        /// <summary>
        /// Returns the contained value as a long.
        /// </summary>
        /// <returns>Contained value as a long.</returns>
        /// <exception cref="InvalidCastException">When the contained value is not a long.</exception>
        public long ToInt64()
        {
            if (Value is int)
                return (int)Value;

            return ValueInt64;
        }

        /// <summary>
        /// Returns the contained value as a string.
        /// </summary>
        /// <returns>Contained value as a string.</returns>
        /// <exception cref="InvalidCastException">When the contained value is not a string.</exception>
        public override string ToString()
        {
            return ValueString;
        }

        /// <summary>
        /// Returns the contained value as a float.
        /// </summary>
        /// <returns>Contained value as a float.</returns>
        /// <exception cref="InvalidCastException">When the contained value is not a float.</exception>
        public float ToSingle()
        {
            return ValueFloat;
        }

        /// <summary>
        /// Returns the contained value as a double.
        /// </summary>
        /// <returns>Contained value as a double.</returns>
        /// <exception cref="InvalidCastException">When the contained value is not a double.</exception>
        public double ToDouble()
        {
            if (Value is float)
                return (float)Value;

            return ValueDouble;
        }

        /// <summary>
        /// Returns the contained value as a decimal.
        /// </summary>
        /// <returns>Contained value as a decimal.</returns>
        /// <exception cref="InvalidCastException">When the contained value is not a decimal.</exception>
        public decimal ToDecimal()
        {
            return (decimal)Value;
        }

        /// <summary>
        /// Returns the contained value as a <see cref="DateTime"/>.
        /// </summary>
        /// <returns>Contained value as a <see cref="DateTime"/>.</returns>
        /// <exception cref="InvalidCastException">When the contained value is not a <see cref="DateTime"/>.</exception>
        public DateTime ToDateTime()
        {
            return (DateTime)Value;
        }

        /// <summary>
        /// Returns the contained value as a byte array.
        /// </summary>
        /// <returns>Contained value as a byte array.</returns>
        /// <exception cref="InvalidCastException">When the contained value is not a byte array.</exception>
        public byte[] ToByteArray()
        {
            return ValueBytes;
        }

        /// <summary>
        /// Returns the hash code of the contained value.
        /// </summary>
        /// <returns>Hash code of the contained value.</returns>
        public override int GetHashCode()
        {
            if (Value == null)
                return 0;
            else
                return Value.GetHashCode();
        }

        /// <summary>
        /// Returns true when the contained value of <paramref name="other"/>
        /// is equal to the contained value of this <see cref="Variant"/>.
        /// </summary>
        /// <param name="other"><see cref="Variant"/> to compare with.</param>
        /// <returns>True when the contained value of <paramref name="other"/>
        /// is equal to the contained value of this <see cref="Variant"/>.</returns>
        public bool Equals(Variant other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return Equals(Value, other.Value);
        }

        /// <summary>
        /// Converts the contained value to a boolean.
        /// </summary>
        /// <param name="variant"><see cref="Variant"/> to convert.</param>
        /// <returns>Contained value as a boolean.</returns>
        public static implicit operator bool(Variant variant)
        {
            return variant.ToBool();
        }

        /// <summary>
        /// Converts a boolean to a <see cref="Variant"/>.
        /// </summary>
        /// <param name="value">Boolean to convert.</param>
        /// <returns><see cref="Variant"/> for the provided boolean.</returns>
        public static implicit operator Variant(bool value)
        {
            return new Variant(value);
        }

        /// <summary>
        /// Converts the contained value to an integer.
        /// </summary>
        /// <param name="variant"><see cref="Variant"/> to convert.</param>
        /// <returns>Contained value as an integer.</returns>
        public static implicit operator int(Variant variant)
        {
            return variant.ToInt32();
        }

        /// <summary>
        /// Converts an integer to a <see cref="Variant"/>.
        /// </summary>
        /// <param name="value">Integer to convert.</param>
        /// <returns><see cref="Variant"/> for the provided integer.</returns>
        public static implicit operator Variant(int value)
        {
            return new Variant(value);
        }

        /// <summary>
        /// Converts the contained value to a long.
        /// </summary>
        /// <param name="variant"><see cref="Variant"/> to convert.</param>
        /// <returns>Contained value as a long.</returns>
        public static implicit operator long(Variant variant)
        {
            return variant.ToInt64();
        }

        /// <summary>
        /// Converts a long to a <see cref="Variant"/>.
        /// </summary>
        /// <param name="value">Long to convert.</param>
        /// <returns><see cref="Variant"/> for the provided long.</returns>
        public static implicit operator Variant(long value)
        {
            return new Variant(value);
        }

        /// <summary>
        /// Converts the contained value to a string.
        /// </summary>
        /// <param name="variant"><see cref="Variant"/> to convert.</param>
        /// <returns>Contained value as a string.</returns>
        public static implicit operator string(Variant variant)
        {
            return variant.ToString();
        }

        /// <summary>
        /// Converts a string to a <see cref="Variant"/>.
        /// </summary>
        /// <param name="value">String to convert.</param>
        /// <returns><see cref="Variant"/> for the provided string.</returns>
        public static implicit operator Variant(string value)
        {
            return new Variant(value);
        }

        /// <summary>
        /// Converts the contained value to a float.
        /// </summary>
        /// <param name="variant"><see cref="Variant"/> to convert.</param>
        /// <returns>Contained value as a float.</returns>
        public static implicit operator float(Variant variant)
        {
            return variant.ToSingle();
        }

        /// <summary>
        /// Converts a float to a <see cref="Variant"/>.
        /// </summary>
        /// <param name="value">Float to convert.</param>
        /// <returns><see cref="Variant"/> for the provided float.</returns>
        public static implicit operator Variant(float value)
        {
            return new Variant(value);
        }

        /// <summary>
        /// Converts the contained value to a double.
        /// </summary>
        /// <param name="variant"><see cref="Variant"/> to convert.</param>
        /// <returns>Contained value as a double.</returns>
        public static implicit operator double(Variant variant)
        {
            return variant.ToDouble();
        }

        /// <summary>
        /// Converts a double to a <see cref="Variant"/>.
        /// </summary>
        /// <param name="value">Double to convert.</param>
        /// <returns><see cref="Variant"/> for the provided double.</returns>
        public static implicit operator Variant(double value)
        {
            return new Variant(value);
        }

        /// <summary>
        /// Converts the contained value to a decimal.
        /// </summary>
        /// <param name="variant"><see cref="Variant"/> to convert.</param>
        /// <returns>Contained value as a decimal.</returns>
        public static implicit operator decimal(Variant variant)
        {
            return variant.ToDecimal();
        }

        /// <summary>
        /// Converts a decimal to a <see cref="Variant"/>.
        /// </summary>
        /// <param name="value">Decimal to convert.</param>
        /// <returns><see cref="Variant"/> for the provided decimal.</returns>
        public static implicit operator Variant(decimal value)
        {
            return new Variant(value);
        }

        /// <summary>
        /// Converts the contained value to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="variant"><see cref="Variant"/> to convert.</param>
        /// <returns>Contained value as a <see cref="DateTime"/>.</returns>
        public static implicit operator DateTime(Variant variant)
        {
            return variant.ToDateTime();
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> to a <see cref="Variant"/>.
        /// </summary>
        /// <param name="value"><see cref="DateTime"/> to convert.</param>
        /// <returns><see cref="Variant"/> for the provided <see cref="DateTime"/>.</returns>
        public static implicit operator Variant(DateTime value)
        {
            return new Variant(value);
        }

        /// <summary>
        /// Converts the contained value to a byte array.
        /// </summary>
        /// <param name="variant"><see cref="Variant"/> to convert.</param>
        /// <returns>Contained value as a byte array.</returns>
        public static implicit operator byte[](Variant variant)
        {
            return variant.ToByteArray();
        }

        /// <summary>
        /// Converts a byte array to a <see cref="Variant"/>.
        /// </summary>
        /// <param name="value">Byte array to convert.</param>
        /// <returns><see cref="Variant"/> for the provided byte array.</returns>
        public static implicit operator Variant(byte[] value)
        {
            return new Variant(value);
        }
    }
}
