//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dse.Mapping.Attributes;

namespace Dse.Test.Unit.Mapping.Pocos
{
    [Table(Name = "tbl1", Keyspace = "ks1", CaseSensitive = true)]
    public class DecoratedTimeSeries
    {
        [PartitionKey(0)]
        [Column("name")]
        public string SensorName { get; set; }

        [PartitionKey(1)]
        public int Slice { get; set; }

        [ClusteringKey]
        public TimeUuid Time { get; set; }

        [Column("val")]
        public double Value { get; set; }

        public string Value2 { get; set; }

        [Ignore]
        public double CalculatedValue { get; set; }
    }
}
