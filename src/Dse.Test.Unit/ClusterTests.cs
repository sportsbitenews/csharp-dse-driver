//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace Dse.Test.Unit
{
    [TestFixture]
    public class ClusterUnitTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Diagnostics.CassandraTraceSwitch.Level = System.Diagnostics.TraceLevel.Verbose;
        }

        [Test]
        public void ClusterAllHostsReturnsOnDisconnectedCluster()
        {
            const string ip = "127.100.100.100";
            var cluster = Cluster.Builder()
             .AddContactPoint(ip)
             .Build();
            //No ring was discovered
            Assert.AreEqual(1, cluster.AllHosts().Count);
            Assert.AreEqual(new IPEndPoint(IPAddress.Parse(ip), 9042), cluster.AllHosts().First().Address);
        }

        [Test]
        public void ClusterConnectThrowsNoHostAvailable()
        {
            var cluster = Cluster.Builder()
             .AddContactPoint("127.100.100.100")
             .Build();
            Assert.Throws<NoHostAvailableException>(() => cluster.Connect());
            Assert.Throws<NoHostAvailableException>(() => cluster.Connect("sample_ks"));
        }

        [Test]
        public void ClusterIsDisposableAfterInitError()
        {
            const string ip = "127.100.100.100";
            var cluster = Cluster.Builder()
             .AddContactPoint(ip)
             .Build();
            Assert.Throws<NoHostAvailableException>(() => cluster.Connect());
            Assert.DoesNotThrow(cluster.Dispose);
        }

        [Test]
        public void Should_Not_Leak_Connections_When_Node_Unreacheable_Test()
        {
            var socketOptions = new SocketOptions().SetReadTimeoutMillis(1).SetConnectTimeoutMillis(1);
            var builder = Cluster.Builder()
                                 .AddContactPoint(TestHelper.UnreachableHostAddress)
                                 .WithSocketOptions(socketOptions);
            const int length = 1000;
            using (var cluster = builder.Build())
            {
                decimal initialLength = GC.GetTotalMemory(true);
                for (var i = 0; i < length; i++)
                {
                    var ex = Assert.Throws<NoHostAvailableException>(() => cluster.Connect());
                    Assert.AreEqual(1, ex.Errors.Count);
                }
                GC.Collect();
                Assert.Less(GC.GetTotalMemory(true) / initialLength, 1.2M,
                    "Should not exceed a 20% (1.2) more than was previously allocated");
            }
        }
    }
}
