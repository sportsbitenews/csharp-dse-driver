﻿//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dse.Data.Linq;
using Dse.Mapping;
using Dse.Serialization;
using Dse.Test.Unit.Mapping.Pocos;
using Moq;
using NUnit.Framework;
using Dse.Tasks;

#pragma warning disable 618

namespace Dse.Test.Unit.Mapping.Linq
{
    [TestFixture]
    public class LinqToCqlUnitTests : MappingTestBase
    {
        [Test]
        public void TestCqlFromLinq()
        {
            var table = SessionExtensions.GetTable<LinqDecoratedEntity>(null);

            Assert.AreEqual(
                (from ent in table select ent).ToString(),
                @"SELECT ""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"" FROM ""x_t"" ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table select ent.f1).ToString(),
                @"SELECT ""x_f1"" FROM ""x_t"" ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table where ent.pk == "koko" select ent.f1).ToString(),
                @"SELECT ""x_f1"" FROM ""x_t"" WHERE ""x_pk"" = ? ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table where ent.pk == "koko" select new { ent.f1, ent.ck2 }).ToString(),
                @"SELECT ""x_f1"", ""x_ck2"" FROM ""x_t"" WHERE ""x_pk"" = ? ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table where ent.pk == "koko" && ent.ck2 == 10 select new { ent.f1, ent.ck2 }).ToString(),
                @"SELECT ""x_f1"", ""x_ck2"" FROM ""x_t"" WHERE ""x_pk"" = ? AND ""x_ck2"" = ? ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table where ent.pk == "koko" && ent.ck2 == 10 select new { ent.f1, ent.ck2 }).Take(10).ToString(),
                @"SELECT ""x_f1"", ""x_ck2"" FROM ""x_t"" WHERE ""x_pk"" = ? AND ""x_ck2"" = ? LIMIT ? ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table where ent.pk == "koko" && ent.ck2 == 10 select new { ent.f1, ent.ck2 }).OrderBy(c => c.ck2).ToString(),
                @"SELECT ""x_f1"", ""x_ck2"" FROM ""x_t"" WHERE ""x_pk"" = ? AND ""x_ck2"" = ? ORDER BY ""x_ck2"" ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table where ent.pk == "koko" && ent.ck2 == 10 select new { ent.f1, ent.ck2, ent.ck1 }).OrderBy(c => c.ck2).OrderByDescending(c => c.ck1).ToString(),
                @"SELECT ""x_f1"", ""x_ck2"", ""x_ck1"" FROM ""x_t"" WHERE ""x_pk"" = ? AND ""x_ck2"" = ? ORDER BY ""x_ck2"", ""x_ck1"" DESC ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table where ent.pk == "koko" && ent.ck2 == 10 select new { ent.f1, ent.ck2, ent.ck1 }).OrderBy(c => c.ck2).OrderByDescending(c => c.ck1).ToString(),
                @"SELECT ""x_f1"", ""x_ck2"", ""x_ck1"" FROM ""x_t"" WHERE ""x_pk"" = ? AND ""x_ck2"" = ? ORDER BY ""x_ck2"", ""x_ck1"" DESC ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table where CqlToken.Create(ent.pk, ent.ck2, ent.ck2) > CqlToken.Create("x", 2) select new { ent.f1, ent.ck2, ent.ck1 }).OrderBy(c => c.ck2).OrderByDescending(c => c.ck1).ToString(),
                @"SELECT ""x_f1"", ""x_ck2"", ""x_ck1"" FROM ""x_t"" WHERE token(""x_pk"", ""x_ck2"", ""x_ck2"") > token(?, ?) ORDER BY ""x_ck2"", ""x_ck1"" DESC ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table where ent.ck2 > ent.ck1 select ent).ToString(),
                @"SELECT ""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"" FROM ""x_t"" WHERE ""x_ck2"" > ""x_ck1"" ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table select ent).FirstOrDefault().ToString(),
                @"SELECT ""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"" FROM ""x_t"" LIMIT ? ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table select ent).First().ToString(),
                @"SELECT ""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"" FROM ""x_t"" LIMIT ? ALLOW FILTERING");

            Assert.AreEqual(
                (from ent in table select ent).Where(e => e.pk.CompareTo("a") > 0).First().ToString(),
                @"SELECT ""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"" FROM ""x_t"" WHERE ""x_pk"" > ? LIMIT ? ALLOW FILTERING");

            Assert.Throws<CqlLinqNotSupportedException>(() =>
                (from ent in table where ent.pk == "x" || ent.ck2 == 1 select ent).ToString());

            Assert.AreEqual(
                (from ent in table where new[] {10, 30, 40}.Contains(ent.ck2) select ent).ToString(),
                @"SELECT ""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"" FROM ""x_t"" WHERE ""x_ck2"" IN (?, ?, ?) ALLOW FILTERING");

            Assert.AreEqual(
                @"SELECT ""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"" FROM ""x_t"" WHERE ""x_ck2"" IN () ALLOW FILTERING",
                (from ent in table where new int[] {}.Contains(ent.ck2) select ent).ToString());

            Assert.AreEqual(
               (from ent in table where new int[] { 10, 30, 40 }.Contains(ent.ck2) select ent).Delete().ToString(),
               @"DELETE FROM ""x_t"" WHERE ""x_ck2"" IN (?, ?, ?)");

            Assert.AreEqual(
                @"INSERT INTO ""x_t"" (""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"") VALUES (?, ?, ?, ?)",
                (table.Insert(new LinqDecoratedEntity() { ck1 = 1, ck2 = 2, f1 = 3, pk = "x" })).ToString());

            Assert.Throws<CqlLinqNotSupportedException>(() => 
                (from ent in table where new int[] {10, 30, 40}.Contains(ent.ck2) select new {x = ent.pk, e = ent}).ToString());
        }

        [Test]
        public void StartsWith_Test()
        {
            var table = new Table<LinqDecoratedWithStringCkEntity>(null);
            var query = table.Where(t => t.pk == "a" && t.ck1.StartsWith("foo") && t.pk == "bar");
            var pocoData = MappingConfiguration.Global.MapperFactory.GetPocoData<LinqDecoratedWithStringCkEntity>();
            var visitor = new CqlExpressionVisitor(pocoData, "x_ts", null);
            object[] parameters;
            var queryCql = visitor.GetSelect(query.Expression, out parameters);

            Assert.That(parameters, Is.EquivalentTo(new[] { "a", "foo", "foo" + Encoding.UTF8.GetString(new byte[] { 0xF4, 0x8F, 0xBF, 0xBF }), "bar" }));
            Assert.AreEqual(@"SELECT ""x_pk"", ""x_ck1"", ""x_f1"" FROM ""x_ts"" WHERE ""x_pk"" = ? AND ""x_ck1"" >= ? AND ""x_ck1"" < ? AND ""x_pk"" = ?", queryCql);
        }

        private class TestObject
        {
            public string Field;
            public string Property { get; set; }
            public TestObject AnotherObjectField;
            public TestObject AnotherObjectProperty { get; set; }
        }

        [Test]
        public void PropertyAccess_Test()
        {
            var testObject = new TestObject
            {
                Property = "a",
                AnotherObjectProperty = new TestObject
                {
                    Property = "b"
                }
            };
            var table = new Table<LinqDecoratedWithStringCkEntity>(null);
            var query = table.Where(t => t.pk == testObject.Property && t.ck1 == testObject.AnotherObjectProperty.Property);
            var pocoData = MappingConfiguration.Global.MapperFactory.GetPocoData<LinqDecoratedWithStringCkEntity>();
            var visitor = new CqlExpressionVisitor(pocoData, "x_ts", null);
            object[] parameters;
            var queryCql = visitor.GetSelect(query.Expression, out parameters);

            Assert.That(parameters, Is.EquivalentTo(new[] { "a", "b" }));
            Assert.AreEqual(@"SELECT ""x_pk"", ""x_ck1"", ""x_f1"" FROM ""x_ts"" WHERE ""x_pk"" = ? AND ""x_ck1"" = ?", queryCql);
        }

        [Test]
        public void FieldAccess_Test()
        {
            var testObject = new TestObject
            {
                Field = "a",
                AnotherObjectField = new TestObject
                {
                    Field = "b"
                }
            };
            var table = new Table<LinqDecoratedWithStringCkEntity>(null);
            var query = table.Where(t => t.pk == testObject.Field && t.ck1 == testObject.AnotherObjectField.Field);
            var pocoData = MappingConfiguration.Global.MapperFactory.GetPocoData<LinqDecoratedWithStringCkEntity>();
            var visitor = new CqlExpressionVisitor(pocoData, "x_ts", null);
            object[] parameters;
            var queryCql = visitor.GetSelect(query.Expression, out parameters);

            Assert.That(parameters, Is.EquivalentTo(new[] { "a", "b" }));
            Assert.AreEqual(@"SELECT ""x_pk"", ""x_ck1"", ""x_f1"" FROM ""x_ts"" WHERE ""x_pk"" = ? AND ""x_ck1"" = ?", queryCql);
        }

        [Test]
        public void MixedFieldAndPropertyAccess_Test()
        {
            var testObject = new TestObject
            {
                AnotherObjectField = new TestObject
                {
                    Property = "a"
                },
                AnotherObjectProperty = new TestObject
                {
                    Field = "b"
                }
            };
            var table = new Table<LinqDecoratedWithStringCkEntity>(null);
            var query = table.Where(t => t.pk == testObject.AnotherObjectField.Property && t.ck1 == testObject.AnotherObjectProperty.Field);
            var pocoData = MappingConfiguration.Global.MapperFactory.GetPocoData<LinqDecoratedWithStringCkEntity>();
            var visitor = new CqlExpressionVisitor(pocoData, "x_ts", null);
            object[] parameters;
            var queryCql = visitor.GetSelect(query.Expression, out parameters);

            Assert.That(parameters, Is.EquivalentTo(new[] { "a", "b" }));
            Assert.AreEqual(@"SELECT ""x_pk"", ""x_ck1"", ""x_f1"" FROM ""x_ts"" WHERE ""x_pk"" = ? AND ""x_ck1"" = ?", queryCql);
        }


        [Test]
        public void Linq_Unlogged_Batch_Test()
        {
            var table = SessionExtensions.GetTable<LinqDecoratedEntity>(null);
            var batch = SessionExtensions.CreateBatch(null, BatchType.Unlogged);
            batch.Append(table.Insert(new LinqDecoratedEntity() { ck1 = 1, ck2 = 2, f1 = 3, pk = "x" }));
            batch.Append(table.Where(ent => new int[] { 10, 30, 40 }.Contains(ent.ck2)).Select(ent => new { f1 = 1223 }).Update());
            batch.Append(table.Where(ent => new int[] { 10, 30, 40 }.Contains(ent.ck2)).Delete());
            Assert.AreEqual(batch.ToString().Replace("\r", ""),
                @"BEGIN UNLOGGED BATCH
INSERT INTO ""x_t"" (""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"") VALUES (?, ?, ?, ?);
UPDATE ""x_t"" SET ""x_f1"" = ? WHERE ""x_ck2"" IN (?, ?, ?);
DELETE FROM ""x_t"" WHERE ""x_ck2"" IN (?, ?, ?);
APPLY BATCH".Replace("\r", ""));
        }

        [Test]
        public void Linq_Logged_Batch_Test()
        {
            var table = SessionExtensions.GetTable<LinqDecoratedEntity>(null);
            var batch = SessionExtensions.CreateBatch(null, BatchType.Logged);
            batch.Append(table.Insert(new LinqDecoratedEntity() { ck1 = 1, ck2 = 2, f1 = 3, pk = "x" }));
            batch.Append(table.Where(ent => new int[] { 10, 30, 40 }.Contains(ent.ck2)).Select(ent => new { f1 = 1223 }).Update());
            batch.Append(table.Where(ent => new int[] { 10, 30, 40 }.Contains(ent.ck2)).Delete());
            Assert.AreEqual(batch.ToString().Replace("\r", ""),
                @"BEGIN BATCH
INSERT INTO ""x_t"" (""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"") VALUES (?, ?, ?, ?);
UPDATE ""x_t"" SET ""x_f1"" = ? WHERE ""x_ck2"" IN (?, ?, ?);
DELETE FROM ""x_t"" WHERE ""x_ck2"" IN (?, ?, ?);
APPLY BATCH".Replace("\r", ""));
        }

        [Test]
        public void Linq_Counter_Batch_Test()
        {
            //TODO: Write actual counter statements
            var table = SessionExtensions.GetTable<CounterTestTable1>(null);
            var batch = SessionExtensions.CreateBatch(null, BatchType.Counter);
            batch.Append(table.Where(ent => ent.RowKey1 == 1 && ent.RowKey2 == 2).Select(_ => new CounterTestTable1 { Value = 1 }).Update());
            batch.Append(table.Where(ent => ent.RowKey1 == 3 && ent.RowKey2 == 4).Select(_ => new CounterTestTable1 { Value = 1 }).Update());
            Assert.AreEqual(batch.ToString().Replace("\r", ""),
                @"BEGIN COUNTER BATCH
UPDATE ""CounterTestTable1"" SET ""Value"" = ""Value"" + ? WHERE ""RowKey1"" = ? AND ""RowKey2"" = ?;
UPDATE ""CounterTestTable1"" SET ""Value"" = ""Value"" + ? WHERE ""RowKey1"" = ? AND ""RowKey2"" = ?;
APPLY BATCH".Replace("\r", ""));
        }

        [Test]
        public void LinqGeneratedUpdateStatementTest()
        {
            var table = SessionExtensions.GetTable<AllTypesEntity>(null);
            string query;
            string expectedQuery;

            query = table
                .Where(r => r.StringValue == "key")
                .Select(r => new AllTypesEntity() { IntValue = 1 })
                .Update()
                .ToString();
            expectedQuery = "UPDATE \"AllTypesEntity\" SET \"IntValue\" = ? WHERE \"StringValue\" = ?";
            Assert.AreEqual(expectedQuery, query);

            Assert.Throws<CqlArgumentException>(() =>
            {
                //Update without a set statement
                //Must include SELECT to project to a new form
                query = table
                    .Where(r => r.StringValue == "key")
                    .Update()
                    .ToString();
            });
        }

        [Test]
        public void TestCqlFromLinqPaxosSupport()
        {
            var table = SessionExtensions.GetTable<LinqDecoratedEntity>(null);

            Assert.AreEqual(
               (table.Insert(new LinqDecoratedEntity() { ck1 = 1, ck2 = 2, f1 = 3, pk = "x" })).IfNotExists().ToString(),
               @"INSERT INTO ""x_t"" (""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"") VALUES (?, ?, ?, ?) IF NOT EXISTS");

            Assert.AreEqual((from ent in table where new int[] { 10, 30, 40 }.Contains(ent.ck2) select new { f1 = 1223 }).UpdateIf((a) => a.f1 == 123).ToString(),
                    @"UPDATE ""x_t"" SET ""x_f1"" = ? WHERE ""x_ck2"" IN (?, ?, ?) IF ""x_f1"" = ?");

            Assert.AreEqual((from ent in table where new int[] { 10, 30, 40 }.Contains(ent.ck2) select ent).DeleteIf((a) => a.f1 == 123).ToString(),
                @"DELETE FROM ""x_t"" WHERE ""x_ck2"" IN (?, ?, ?) IF ""x_f1"" = ?");

            Assert.AreEqual((from ent in table where new int[] { 10, 30, 40 }.Contains(ent.ck2) select ent).Delete().IfExists().ToString(),
                @"DELETE FROM ""x_t"" WHERE ""x_ck2"" IN (?, ?, ?) IF EXISTS ");
        }

        [Test]
        public void TestCqlNullValuesLinqSupport()
        {
            var table = SessionExtensions.GetTable<LinqDecoratedEntity>(null);

            Assert.AreEqual(
                @"INSERT INTO ""x_t"" (""x_pk"", ""x_ck1"", ""x_ck2"", ""x_f1"") VALUES (?, ?, ?, ?)",
                (table.Insert(new LinqDecoratedEntity() { ck1 = null, ck2 = 2, f1 = 3, pk = "x" })).ToString());

            Assert.AreEqual(
                @"UPDATE ""x_t"" SET ""x_f1"" = ? WHERE ""x_ck1"" IN (?, ?, ?)",
                (from ent in table where new int?[] { 10, 30, 40 }.Contains(ent.ck1) select new { f1 = 1223 }).Update().ToString());

            Assert.AreEqual(
                @"UPDATE ""x_t"" SET ""x_f1"" = ?, ""x_ck1"" = ? WHERE ""x_ck1"" IN (?, ?, ?)",
                (from ent in table where new int?[] { 10, 30, 40 }.Contains(ent.ck1) select new LinqDecoratedEntity() { f1 = 1223, ck1 = null }).Update().ToString());

            Assert.AreEqual(
                @"UPDATE ""x_t"" SET ""x_f1"" = ?, ""x_ck1"" = ? WHERE ""x_ck1"" = ?",
                (from ent in table where ent.ck1 == 1 select new LinqDecoratedEntity() { f1 = 1223, ck1 = null }).Update().ToString());

            Assert.AreEqual(
                @"UPDATE ""x_t"" SET ""x_f1"" = ?, ""x_ck1"" = ? WHERE ""x_ck1"" IN (?, ?, ?) IF ""x_f1"" = ?",
                (from ent in table where new int?[] { 10, 30, 40 }.Contains(ent.ck1) select new { f1 = 1223, ck1 = (int?)null }).UpdateIf((a) => a.f1 == 123).ToString());
        }

        /// <summary>
        /// Tests the Linq to CQL generated where clause 
        /// </summary>
        [Test]
        public void LinqSelectWhereTest()
        {
            var sessionMock = new Mock<ISession>();
            var session = sessionMock.Object;

            var table = session.GetTable<AllTypesEntity>();
            var date = new DateTime(1975, 1, 1);
            var linqQueries = new List<CqlQuery<AllTypesEntity>>()
            {
                (from ent in table where ent.BooleanValue == true select ent),
                (from ent in table where ent.DateTimeValue < date select ent),
                (from ent in table where ent.DateTimeValue >= date select ent),
                (from ent in table where ent.IntValue == 0 select ent),
                (from ent in table where ent.StringValue == "Hello world" select ent)
                
            };
            var expectedCqlQueries = new List<string>()
            {
                @"SELECT ""BooleanValue"", ""DateTimeValue"", ""DecimalValue"", ""DoubleValue"", ""Int64Value"", ""IntValue"", ""StringValue"", ""UuidValue"" FROM ""AllTypesEntity"" WHERE ""BooleanValue"" = ?",
                @"SELECT ""BooleanValue"", ""DateTimeValue"", ""DecimalValue"", ""DoubleValue"", ""Int64Value"", ""IntValue"", ""StringValue"", ""UuidValue"" FROM ""AllTypesEntity"" WHERE ""DateTimeValue"" < ?",
                @"SELECT ""BooleanValue"", ""DateTimeValue"", ""DecimalValue"", ""DoubleValue"", ""Int64Value"", ""IntValue"", ""StringValue"", ""UuidValue"" FROM ""AllTypesEntity"" WHERE ""DateTimeValue"" >= ?",
                @"SELECT ""BooleanValue"", ""DateTimeValue"", ""DecimalValue"", ""DoubleValue"", ""Int64Value"", ""IntValue"", ""StringValue"", ""UuidValue"" FROM ""AllTypesEntity"" WHERE ""IntValue"" = ?",
                @"SELECT ""BooleanValue"", ""DateTimeValue"", ""DecimalValue"", ""DoubleValue"", ""Int64Value"", ""IntValue"", ""StringValue"", ""UuidValue"" FROM ""AllTypesEntity"" WHERE ""StringValue"" = ?"
            };
            var actualCqlQueries = new List<string>();
            sessionMock
                .Setup(s => s.ExecuteAsync(It.IsAny<IStatement>()))
                .Returns(Task<RowSet>.Factory.StartNew(() => new RowSet()));
            sessionMock
                .Setup(s => s.PrepareAsync(It.IsAny<string>()))
                .Callback<string>(actualCqlQueries.Add)
                .Returns(TaskHelper.ToTask(GetPrepared("Mock query")));

            //Execute all linq queries
            foreach (var q in linqQueries)
            {
                q.Execute();
            }
            sessionMock.Verify();

            Assert.AreEqual(expectedCqlQueries.Count, actualCqlQueries.Count);
            //Check that all expected queries and actual queries are equal
            for (var i = 0; i < expectedCqlQueries.Count; i++)
            {
                Assert.AreEqual(
                    expectedCqlQueries[i],
                    actualCqlQueries[i],
                    "Expected Cql query and generated CQL query by Linq do not match.");
            }
        }

        [Table]
        private class AllowFilteringTestTable
        {
            [PartitionKey]
            public int RowKey { get; set; }

            [ClusteringKey(1)]
            [SecondaryIndex]
            public string ClusteringKey { get; set; }

            public decimal Value { get; set; }
        }

        [Test]
        public void AllowFilteringTest()
        {
            var table = SessionExtensions.GetTable<AllowFilteringTestTable>(null);
            var key1 = "x";
            var cqlQuery = table
                .Where(item => item.ClusteringKey == key1 && item.Value == 1M)
                .AllowFiltering();

            Assert.That(cqlQuery, Is.Not.Null);
            Assert.That(cqlQuery.ToString(), Is.StringEnding("ALLOW FILTERING"));
            Trace.WriteLine(cqlQuery.ToString());
        }

        [Table]
        private class CounterTestTable1
        {
            [PartitionKey]
            public int RowKey1 { get; set; }

            [ClusteringKey(0)]
            public int RowKey2 { get; set; }

            [Counter]
            public long Value { get; set; }
        }

        [Table]
        public class CounterTestTable2
        {
            [PartitionKey(0)]
            public int RowKey1 { get; set; }

            [PartitionKey(1)]
            public int RowKey2 { get; set; }

            [ClusteringKey(0)]
            public int CKey1 { get; set; }

            [Counter]
            public long Value { get; set; }
        }

        [Test]
        public void CreateTableCounterTest()
        {
            var actualCqlQueries = new List<string>();
            var sessionMock = new Mock<ISession>(MockBehavior.Strict);
            var config = new Configuration();
            var metadata = new Metadata(config);
            var ccMock = new Mock<IMetadataQueryProvider>(MockBehavior.Strict);
            ccMock.Setup(cc => cc.Serializer).Returns(new Serializer(ProtocolVersion.MaxSupported));
            metadata.ControlConnection = ccMock.Object;
            var clusterMock = new Mock<ICluster>();
            clusterMock.Setup(c => c.Metadata).Returns(metadata);
            sessionMock.Setup(s => s.Cluster).Returns(clusterMock.Object);
            sessionMock
                .Setup(s => s.Execute(It.IsAny<string>()))
                .Returns(() => new RowSet())
                .Callback<string>(actualCqlQueries.Add)
                .Verifiable();

            var session = sessionMock.Object;
            var table1 = SessionExtensions.GetTable<CounterTestTable1>(session);
            table1.CreateIfNotExists();

            var table2 = SessionExtensions.GetTable<CounterTestTable2>(session);
            table2.CreateIfNotExists();

            sessionMock.Verify();
            Assert.Greater(actualCqlQueries.Count, 0);
            Assert.AreEqual("CREATE TABLE \"CounterTestTable1\" (\"RowKey1\" int, \"RowKey2\" int, \"Value\" counter, PRIMARY KEY (\"RowKey1\", \"RowKey2\"))", actualCqlQueries[0]);
            Assert.AreEqual("CREATE TABLE \"CounterTestTable2\" (\"RowKey1\" int, \"RowKey2\" int, \"CKey1\" int, \"Value\" counter, PRIMARY KEY ((\"RowKey1\", \"RowKey2\"), \"CKey1\"))", actualCqlQueries[1]);
        }

        [Test]
        public void VirtualPropertiesTest()
        {
            var table = SessionExtensions.GetTable<InheritedEntity>(null);
            var query1 = table.Where(e => e.Id == 10);
            Assert.AreEqual("SELECT \"Id\", \"Description\", \"Name\" FROM \"InheritedEntity\" WHERE \"Id\" = ?", query1.ToString());
            var query2 = (from e in table where e.Id == 1 && e.Name == "MyName" select new { e.Id, e.Name, e.Description });
            Assert.AreEqual("SELECT \"Id\", \"Name\", \"Description\" FROM \"InheritedEntity\" WHERE \"Id\" = ? AND \"Name\" = ?", query2.ToString());
            var sessionMock = new Mock<ISession>(MockBehavior.Strict);
            var config = new Configuration();
            var metadata = new Metadata(config);
            var ccMock = new Mock<IMetadataQueryProvider>(MockBehavior.Strict);
            ccMock.Setup(cc => cc.Serializer).Returns(new Serializer(ProtocolVersion.MaxSupported));
            metadata.ControlConnection = ccMock.Object;
            var clusterMock = new Mock<ICluster>();
            clusterMock.Setup(c => c.Metadata).Returns(metadata);
            sessionMock.Setup(s => s.Cluster).Returns(clusterMock.Object);
            string createQuery = null;
            sessionMock
                .Setup(s => s.Execute(It.IsAny<string>()))
                .Returns(() => new RowSet())
                .Callback<string>(q => createQuery = q)
                .Verifiable();
            var table2 = SessionExtensions.GetTable<InheritedEntity>(sessionMock.Object);
            table2.CreateIfNotExists();
            Assert.AreEqual("CREATE TABLE \"InheritedEntity\" (\"Id\" int, \"Description\" text, \"Name\" text, PRIMARY KEY (\"Id\"))", createQuery);
        }

        [Test]
        public void LinqGeneratedUpdateStatementForCounterTest()
        {
            var table = SessionExtensions.GetTable<CounterTestTable1>(null);
            var query = table
                .Where(r => r.RowKey1 == 5 && r.RowKey2 == 6)
                .Select(r => new CounterTestTable1() { Value = 2 })
                .Update()
                .ToString();
            Assert.AreEqual("UPDATE \"CounterTestTable1\" SET \"Value\" = \"Value\" + ? WHERE \"RowKey1\" = ? AND \"RowKey2\" = ?", query);
            int x = 4;
            query = table
                .Where(r => r.RowKey1 == 5 && r.RowKey2 == 6)
                .Select(r => new CounterTestTable1() { Value = x })
                .Update()
                .ToString();
            Assert.AreEqual("UPDATE \"CounterTestTable1\" SET \"Value\" = \"Value\" + ? WHERE \"RowKey1\" = ? AND \"RowKey2\" = ?", query);
        }

        private class InsertNullTable
        {
            public int Key { get; set; }

            public string Value { get; set; }
        }

        [Test]
        public void Insert_With_Nulls_Test()
        {
            var table = new Table<InsertNullTable>(null, new MappingConfiguration());
            var row = new InsertNullTable { Key = 101, Value = null };

            var cqlInsert = table.Insert(row);
            object[] values;
            var cql = cqlInsert.GetCqlAndValues(out values);

            Assert.AreEqual("INSERT INTO InsertNullTable (Key, Value) VALUES (?, ?)", cql);
            Assert.AreEqual(2, values.Length);
            Assert.AreEqual(101, values[0]);
            Assert.AreEqual(null, values[1]);
        }

        [Test]
        public void Insert_Without_Nulls_Test()
        {
            var table = new Table<InsertNullTable>(null, new MappingConfiguration());
            var row = new InsertNullTable { Key = 102, Value = null };

            var cqlInsert = table.Insert(row, false);
            object[] values;
            var cql = cqlInsert.GetCqlAndValues(out values);

            Assert.AreEqual("INSERT INTO InsertNullTable (Key) VALUES (?)", cql);
            Assert.AreEqual(1, values.Length);
            Assert.AreEqual(102, values[0]);
        }

        [Test]
        public void Insert_Without_Nulls_With_Table_And_Keyspace_Name_Test()
        {
            var table = new Table<InsertNullTable>(null, new MappingConfiguration(), "tbl1", "ks100");
            var row = new InsertNullTable { Key = 102, Value = null };

            var cqlInsert = table.Insert(row, false);
            object[] values;
            var cql = cqlInsert.GetCqlAndValues(out values);

            Assert.AreEqual("INSERT INTO ks100.tbl1 (Key) VALUES (?)", cql);
            Assert.AreEqual(1, values.Length);
            Assert.AreEqual(102, values[0]);
        }

        [Test]
        public void Insert_Without_Nulls_With_Table_Test()
        {
            var table = new Table<InsertNullTable>(null, new MappingConfiguration(), "tbl1");
            var row = new InsertNullTable { Key = 110, Value = null };

            var cqlInsert = table.Insert(row, false);
            object[] values;
            var cql = cqlInsert.GetCqlAndValues(out values);

            Assert.AreEqual("INSERT INTO tbl1 (Key) VALUES (?)", cql);
            Assert.AreEqual(1, values.Length);
            Assert.AreEqual(110, values[0]);
        }

        [Test]
        public void Insert_IfNotExists_Test()
        {
            var table = SessionExtensions.GetTable<AllTypesDecorated>(null);
            var uuid = Guid.NewGuid();
            var row = new AllTypesDecorated { Int64Value = 202, UuidValue = uuid};

            var cqlInsert = table.Insert(row).IfNotExists();
            object[] values;
            var cql = cqlInsert.GetCql(out values);

            StringAssert.EndsWith("IF NOT EXISTS", cql);
        }

        [Test]
        public void Insert_IfNotExists_With_Ttl_And_Timestamp_Test()
        {
            var table = new Table<InsertNullTable>(null, new MappingConfiguration());
            var row = new InsertNullTable { Key = 103, Value = null };

            var timestamp = DateTimeOffset.UtcNow;
            var cqlInsert = table.Insert(row);
            cqlInsert.IfNotExists();
            cqlInsert.SetTTL(86401);
            cqlInsert.SetTimestamp(timestamp);
            object[] values;
            var cql = cqlInsert.GetCqlAndValues(out values);

            Assert.AreEqual("INSERT INTO InsertNullTable (Key, Value) VALUES (?, ?) IF NOT EXISTS USING TTL ? AND TIMESTAMP ?", cql);
            Assert.AreEqual(4, values.Length);
            Assert.AreEqual(103, values[0]);
            Assert.AreEqual(null, values[1]);
            Assert.AreEqual(86401, values[2]);
            Assert.AreEqual((timestamp - new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero)).Ticks / 10, values[3]);
        }

        [Test]
        public void Insert_IfNotExists_Without_Nulls_With_Timestamp_Test()
        {
            var table = new Table<InsertNullTable>(null, new MappingConfiguration());
            var row = new InsertNullTable { Key = 104, Value = null };

            var timestamp = DateTimeOffset.UtcNow;
            var cqlInsert = table.Insert(row, false);
            cqlInsert.IfNotExists();
            cqlInsert.SetTimestamp(timestamp);
            object[] values;
            var cql = cqlInsert.GetCqlAndValues(out values);

            Assert.AreEqual("INSERT INTO InsertNullTable (Key) VALUES (?) IF NOT EXISTS USING TIMESTAMP ?", cql);
            Assert.AreEqual(2, values.Length);
            Assert.AreEqual(104, values[0]);
            Assert.AreEqual((timestamp - new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero)).Ticks / 10, values[1]);
        }

        [Test]
        public void EmptyListTest()
        {
            var table = SessionExtensions.GetTable<LinqDecoratedEntity>(null);
            var keys = new string[0];
            var query = table.Where(item => keys.Contains(item.pk));

            Assert.True(query.ToString().Contains("\"x_pk\" IN ()"), "The query must contain an empty IN statement");
        }

        private class BaseEntity
        {
            [PartitionKey]
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
        }

        private class InheritedEntity : BaseEntity
        {
            [PartitionKey]
            public override int Id
            {
                get
                {
                    return base.Id;
                }
                set
                {
                    base.Id = value;
                }
            }

            public string Description { get; set; }
        }

        [Test]
        public void Select_Specific_Columns()
        {
            var table = SessionExtensions.GetTable<LinqDecoratedEntity>(null);
            var query = table.Select(t => new LinqDecoratedEntity { f1 = t.f1, pk = t.pk });
            Assert.AreEqual(@"SELECT ""x_f1"", ""x_pk"" FROM ""x_t"" ALLOW FILTERING", query.ToString());
        }

        [Test]
        public void Select_Count()
        {
            var table = SessionExtensions.GetTable<LinqDecoratedEntity>(null);
            Assert.AreEqual(
                (from ent in table select ent).Count().ToString(),
                @"SELECT count(*) FROM ""x_t"" ALLOW FILTERING");
        }

        [Test]
        public void Select_One_Columns()
        {
            var table = SessionExtensions.GetTable<LinqDecoratedEntity>(null);
            var query = table.Select(t => t.f1);
            Assert.AreEqual(@"SELECT ""x_f1"" FROM ""x_t"" ALLOW FILTERING", query.ToString());
        }

        [Test]
        public void Select_OrderBy_Columns()
        {
            var table = SessionExtensions.GetTable<AllTypesEntity>(null);
            var query = table.OrderBy(t => t.UuidValue).OrderByDescending(t => t.DateTimeValue);
            Assert.AreEqual(@"SELECT ""BooleanValue"", ""DateTimeValue"", ""DecimalValue"", ""DoubleValue"", ""Int64Value"", ""IntValue"", ""StringValue"", ""UuidValue"" FROM ""AllTypesEntity"" ORDER BY ""UuidValue"", ""DateTimeValue"" DESC", query.ToString());
        }

        [Test]
        public void Select_Where_Contains()
        {
            var table = SessionExtensions.GetTable<AllTypesDecorated>(null);
            var ids = new []{ 1, 2, 3};
            var query = table.Where(t => ids.Contains(t.IntValue) && t.Int64Value == 10);
            Assert.AreEqual(@"SELECT ""boolean_VALUE"", ""datetime_VALUE"", ""decimal_VALUE"", ""double_VALUE"", ""int64_VALUE"", ""int_VALUE"", ""string_VALUE"", ""timeuuid_VALUE"", ""uuid_VALUE"" FROM ""atd"" WHERE ""int_VALUE"" IN (?, ?, ?) AND ""int64_VALUE"" = ?", query.ToString());
        }

        [Test]
        public void DeleteIf_With_Where_Clause()
        {
            var table = SessionExtensions.GetTable<AllTypesDecorated>(null);
            var query = table
                .Where(t => t.Int64Value == 1)
                .DeleteIf(t => t.StringValue == "conditional!");
            Assert.AreEqual(
                @"DELETE FROM ""atd"" WHERE ""int64_VALUE"" = ? IF ""string_VALUE"" = ?",
                query.ToString());
            Trace.TraceInformation(query.ToString());
        }
	}
}
