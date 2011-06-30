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
    }
}
