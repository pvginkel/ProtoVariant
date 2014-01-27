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
            var variant = new Variant(1);

            var bytes = GetBytes(variant);

            variant = GetInstance<Variant>(bytes);

            Assert.AreEqual(variant.Value, 1);
        }

        [Test]
        public void Test_sizes()
        {
            Assert.AreEqual(2, GetBytes(new Variant(0)).Length);
            Assert.AreEqual(0, GetBytes(new Variant(null)).Length);
            Assert.AreEqual(2, GetBytes(new Variant(1)).Length);
            Assert.AreEqual(2, GetBytes(new Variant(true)).Length);
            Assert.AreEqual(7, GetBytes(new Variant("Foo")).Length);
            Assert.AreEqual(23, GetBytes(new Variant(DateTime.Now)).Length);
            Assert.AreEqual(5, GetBytes(new Variant(1m)).Length);
            Assert.AreEqual(13, GetBytes(new Variant(12345.678m)).Length);
            Assert.AreEqual(5, GetBytes(new Variant(1d)).Length);
            Assert.AreEqual(5, GetBytes(new Variant(1f)).Length);
            Assert.AreEqual(7, GetBytes(new Variant(Single.NaN)).Length);
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
            //Assert.AreEqual(12345.6789f, PerformFullRoundtrip(12345.6789f));
            Assert.AreEqual(Single.Epsilon, PerformFullRoundtrip(Single.Epsilon));
            //Assert.AreEqual(Single.MinValue, PerformFullRoundtrip(Single.MinValue));
            //Assert.AreEqual(Single.MaxValue, PerformFullRoundtrip(Single.MaxValue));
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
            //Assert.AreEqual(Double.MinValue, PerformFullRoundtrip(Double.MinValue));
            //Assert.AreEqual(Double.MaxValue, PerformFullRoundtrip(Double.MaxValue));
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

            Assert.AreEqual(StripMilliseconds(now), PerformFullRoundtrip(now));
            Assert.AreEqual(StripMilliseconds(DateTime.MinValue), StripMilliseconds((DateTime)PerformFullRoundtrip(DateTime.MinValue)));
            Assert.AreEqual(StripMilliseconds(DateTime.MaxValue), StripMilliseconds((DateTime)PerformFullRoundtrip(DateTime.MaxValue)));
        }

        private DateTime StripMilliseconds(DateTime value)
        {
            return new DateTime(
                value.Year,
                value.Month,
                value.Day,
                value.Hour,
                value.Minute,
                value.Second
            );
        }

        [Test]
        [Ignore("Variant doesn't (yet) support bytes")]
        public void Test_bytes()
        {
            Assert.AreEqual(new byte[0], PerformFullRoundtrip(new byte[0]));
            Assert.AreEqual(new byte[] { 1, 2, 3 }, PerformFullRoundtrip(new byte[] { 1, 2, 3 }));
        }

        private object PerformFullRoundtrip(object value)
        {
            var variant = new Variant(value);

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
