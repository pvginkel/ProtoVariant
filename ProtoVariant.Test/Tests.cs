using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ProtoBuf;
using System.IO;

namespace ProtoVariant.Test
{
    [TestFixture]
    class Tests
    {
        [Test]
        public void Simple_roundtrip()
        {
            var variant = Variant.Create(1);

            var bytes = GetBytes(variant);

            variant = GetInstance<Variant>(bytes);

            Assert.AreEqual(variant.Value, 1);
        }

        [Test]
        public void Test_sizes()
        {
            Assert.AreEqual(2, GetBytes(Variant.Create(0)).Length);
            Assert.AreEqual(2, GetBytes(Variant.Create(null)).Length);
            Assert.AreEqual(2, GetBytes(Variant.Create(1)).Length);
            Assert.AreEqual(2, GetBytes(Variant.Create(true)).Length);
            Assert.AreEqual(5, GetBytes(Variant.Create("Foo")).Length);
            Assert.AreEqual(21, GetBytes(Variant.Create(DateTime.Now)).Length);
            Assert.AreEqual(3, GetBytes(Variant.Create(1m)).Length);
            Assert.AreEqual(11, GetBytes(Variant.Create(12345.678m)).Length);
            Assert.AreEqual(9, GetBytes(Variant.Create(1d)).Length);
            Assert.AreEqual(5, GetBytes(Variant.Create(1f)).Length);
            Assert.AreEqual(5, GetBytes(Variant.Create(Single.NaN)).Length);
        }

        [Test]
        public void Test_bool()
        {
            Assert.AreEqual(true, PerformFullRoundtrip(true));
            Assert.AreEqual(false, PerformFullRoundtrip(false));
        }

        [Test]
        public void Test_int32()
        {
            Assert.AreEqual(0, PerformFullRoundtrip(0));
            Assert.AreEqual(1, PerformFullRoundtrip(1));
            Assert.AreEqual(Int32.MaxValue, PerformFullRoundtrip(Int32.MaxValue));
            Assert.AreEqual(Int32.MinValue, PerformFullRoundtrip(Int32.MinValue));
        }

        [Test]
        public void Test_int64()
        {
            Assert.AreEqual(0L, PerformFullRoundtrip(0L));
            Assert.AreEqual(1L, PerformFullRoundtrip(1L));
            Assert.AreEqual(Int64.MaxValue, PerformFullRoundtrip(Int64.MaxValue));
            Assert.AreEqual(Int64.MinValue, PerformFullRoundtrip(Int64.MinValue));
        }

        [Test]
        public void Test_float()
        {
            Assert.AreEqual(0f, PerformFullRoundtrip(0f));
            Assert.AreEqual(1f, PerformFullRoundtrip(1f));
            Assert.AreEqual(12345.6789f, PerformFullRoundtrip(12345.6789f));
            Assert.AreEqual(Single.Epsilon, PerformFullRoundtrip(Single.Epsilon));
            Assert.AreEqual(Single.MinValue, PerformFullRoundtrip(Single.MinValue));
            Assert.AreEqual(Single.MaxValue, PerformFullRoundtrip(Single.MaxValue));
            Assert.AreEqual(Single.NaN, PerformFullRoundtrip(Single.NaN));
            Assert.AreEqual(Single.NegativeInfinity, PerformFullRoundtrip(Single.NegativeInfinity));
            Assert.AreEqual(Single.PositiveInfinity, PerformFullRoundtrip(Single.PositiveInfinity));
        }

        [Test]
        public void Test_double()
        {
            Assert.AreEqual(0d, PerformFullRoundtrip(0d));
            Assert.AreEqual(1d, PerformFullRoundtrip(1d));
            Assert.AreEqual(12345.6789d, PerformFullRoundtrip(12345.6789d));
            Assert.AreEqual(Double.Epsilon, PerformFullRoundtrip(Double.Epsilon));
            Assert.AreEqual(Double.MinValue, PerformFullRoundtrip(Double.MinValue));
            Assert.AreEqual(Double.MaxValue, PerformFullRoundtrip(Double.MaxValue));
            Assert.AreEqual(Double.NaN, PerformFullRoundtrip(Double.NaN));
            Assert.AreEqual(Double.NegativeInfinity, PerformFullRoundtrip(Double.NegativeInfinity));
            Assert.AreEqual(Double.PositiveInfinity, PerformFullRoundtrip(Double.PositiveInfinity));
        }

        [Test]
        public void Test_null()
        {
            Assert.AreEqual(null, PerformFullRoundtrip(null));
        }

        [Test]
        public void Test_string()
        {
            Assert.AreEqual("", PerformFullRoundtrip(""));
            Assert.AreEqual("Foo", PerformFullRoundtrip("Foo"));
            Assert.AreEqual(new String('w', 10000), PerformFullRoundtrip(new String('w', 10000)));
        }

        [Test]
        public void Test_decimal()
        {
            Assert.AreEqual(0m, PerformFullRoundtrip(0m));
            Assert.AreEqual(1m, PerformFullRoundtrip(1m));
            Assert.AreEqual(12345.6789m, PerformFullRoundtrip(12345.6789m));
            Assert.AreEqual(Decimal.MinValue, PerformFullRoundtrip(Decimal.MinValue));
            Assert.AreEqual(Decimal.MaxValue, PerformFullRoundtrip(Decimal.MaxValue));
        }

        [Test]
        public void Test_datetime()
        {
            var now = DateTime.Now;

            Assert.AreEqual(RoundDateTime(now), PerformFullRoundtrip(RoundDateTime(now)));
            Assert.AreEqual(RoundDateTime(DateTime.MinValue), PerformFullRoundtrip(RoundDateTime(DateTime.MinValue)));
            Assert.AreEqual(RoundDateTime(DateTime.MaxValue), PerformFullRoundtrip(RoundDateTime(DateTime.MaxValue)));
        }

        private DateTime RoundDateTime(DateTime dateTime)
        {
            // Milliseconds are not serialized.

            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }

        private object PerformFullRoundtrip(object value)
        {
            var variant = Variant.Create(value);

            var newVariant = GetInstance<Variant>(GetBytes(variant));

            return newVariant.Value;
        }

        private byte[] GetBytes<T>(T instance)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize<T>(stream, instance);

                stream.Flush();

                return stream.ToArray();
            }
        }

        private T GetInstance<T>(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }
    }
}
