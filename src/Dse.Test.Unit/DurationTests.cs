﻿//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Linq;
using Dse.Serialization;
using NUnit.Framework;

namespace Dse.Test.Unit
{
    public class DurationTests : BaseUnitTest
    {
        private static readonly Tuple<Duration, string, string>[] Values =
        {
            // Duration representation, hex value, standard format
            Tuple.Create(new Duration(0, 0, 1L), "000002", "1ns"),
            Tuple.Create(new Duration(0, 0, 128), "00008100", "128ns"),
            Tuple.Create(new Duration(0, 0, 200), "00008190", "200ns"),
            Tuple.Create(new Duration(0, 0, 2), "000004", "2ns"),
            Tuple.Create(new Duration(0, 0, 256), "00008200", "256ns"),
            Tuple.Create(new Duration(0, 0, 33001), "0000c101d2", "33us1ns"),
            Tuple.Create(new Duration(0, 0, -1), "000001", "-1ns"),
            Tuple.Create(new Duration(0, 0, -33001), "0000c101d1", "-33µs1ns"),
            Tuple.Create(new Duration(0, 0, 0L), "000000", "0"),
            Tuple.Create(new Duration(0, 0, 2251799813685279), "0000fe1000000000003e", "625h29m59s813ms685us279ns"),
            Tuple.Create(new Duration(-1, -1, -1), "010101", "-1mo1d1ns"),
            Tuple.Create(new Duration(1, 1, 1), "020202", "1mo1d1ns"),
            Tuple.Create(new Duration(2, 15, 0), "041e00", "2mo15d"),
            Tuple.Create(new Duration(0, 14, 0), "001c00", "14d"),
            Tuple.Create(new Duration(257, 0, 0), "82020000", "21y5mo"),
            Tuple.Create(new Duration(0, 2, 120000000000), "0004f837e11d6000", "2d2m")
        };

        private static readonly Tuple<Duration, string>[] IsoFormatValues =
        {
            Tuple.Create(new Duration(2, 15, 0), "P2M15D"),
            Tuple.Create(new Duration(0, 14, 0), "P14D"),
            Tuple.Create(new Duration(257, 0, 0), "P21Y5M"),
            Tuple.Create(new Duration(0, 2, 120000000000), "P2DT2M"),
            Tuple.Create(new Duration(0, 0, 1105000), "PT0.001105S")
        };

        [Test]
        public void Parse_Should_Read_Standard_Format_Test()
        {
            foreach (var value in Values)
            {
                Assert.AreEqual(Duration.Parse(value.Item3), value.Item1);
            }
        }
        
        [Test]
        public void Parse_Should_Parse_And_Output_Iso_Format_Test()
        {
            foreach (var value in IsoFormatValues)
            {
                var duration = Duration.Parse(value.Item2);
                Assert.AreEqual(value.Item1, duration);
                Assert.AreEqual(value.Item2, duration.ToIsoString());
            }
        }

        [Test]
        public void Parse_Should_Read_Iso_Week_Format_Test()
        {
            Assert.AreEqual(Duration.Parse("P1W"), new Duration(0, 7, 0));
            Assert.AreEqual(Duration.Parse("P2W"), new Duration(0, 14, 0));
        }

        [Test]
        public void ToString_Should_Return_Standard_Format_Test()
        {
            foreach (var value in Values)
            {
                var expected = value.Item3.Replace("µs", "us");
                Assert.AreEqual(value.Item1.ToString(), expected);
            }
        }

        [Test]
        public void Equality_Tests()
        {
            foreach (var value in Values)
            {
                var a = value.Item1;
                var b = new Duration(a.Months, a.Days, a.Nanoseconds);
                Assert.AreEqual(a, b);
                Assert.True(a == b);
                Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
                Assert.False(a.Equals(null));
            }
        }

        [Test]
        public void DurationSerializer_Should_Serialize()
        {
            var serializer = new DurationSerializer(true);
            foreach (var value in Values)
            {
                Assert.AreEqual(value.Item2, ToHex(serializer.Serialize(4, value.Item1)));
            }
        }

        [Test]
        public void DurationSerializer_Should_Deserialize()
        {
            var serializer = new DurationSerializer(true);
            foreach (var value in Values)
            {
                var buffer = FromHex(value.Item2);
                Assert.AreEqual(value.Item1, serializer.Deserialize(4, buffer, 0, buffer.Length, null));
            }
        }

        [Test]
        public void ToTimeSpan_Should_Throw_For_Month_Not_Equals_To_Zero()
        {
            var values = new[]
            {
                new Duration(1, 0, 0),
                new Duration(-10, 0, 0),
                new Duration(500, 1, 1L)
            };
            foreach (var value in values)
            {
                Assert.Throws<InvalidOperationException>(() => value.ToTimeSpan());
            }
        }

        [Test]
        public void ToTimeSpan_FromTimeSpan_Test()
        {
            var values = Values.Select(t => t.Item1).Where(d => d.Months == 0L && d.Nanoseconds%100L == 0L);
            foreach (var value in values)
            {
                var timespan = value.ToTimeSpan();
                Assert.AreEqual(value, Duration.FromTimeSpan(timespan));
            }
        }
    }
}
