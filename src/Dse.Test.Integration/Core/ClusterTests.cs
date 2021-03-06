//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Dse.Test.Integration.TestClusterManagement;
using Dse.Test.Unit;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dse.Test.Integration.Core
{
    [Category("short"), TestFixture]
    public class ClusterTests : TestGlobals
    {
        private ITestCluster _testCluster;

        [TearDown]
        public void TestTearDown()
        {
            if (_testCluster != null)
            {
                _testCluster.Remove();
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public Task Cluster_Connect_Should_Initialize_Loadbalancing_With_ControlConnection_Address_Set(bool asyncConnect)
        {
            _testCluster = TestClusterManager.CreateNew(2);
            var lbp = new TestLoadBalancingPolicy();
            var cluster = Cluster.Builder()
                .AddContactPoint(_testCluster.InitialContactPoint)
                .WithLoadBalancingPolicy(lbp)
                .Build();
            return Run(cluster, asyncConnect, session =>
            {
                Assert.NotNull(lbp.ControlConnectionHost);
                Assert.AreEqual(IPAddress.Parse(_testCluster.InitialContactPoint),
                    lbp.ControlConnectionHost.Address.Address);
            });
        }

        /// Tests that MaxProtocolVersion is honored when set
        ///
        /// Cluster_Should_Honor_MaxProtocolVersion_Set tests that the MaxProtocolVersion set when building a cluster is
        /// honored properly by the driver. It first verifies that the default MaxProtocolVersion is the maximum available by
        /// the driver (ProtocolVersion 4 as of driver 3.0.1). It then verifies that a set MaxProtocolVersion is honored when
        /// connecting to a Cassandra cluster. It also verifies that setting an arbitary MaxProtocolVersion is allowed, as the
        /// ProtocolVersion will be negotiated down upon first connection. Finally, it verifies that a MaxProtocolVersion is
        /// not valid.
        ///
        /// @expected_errors ArgumentException When MaxProtocolVersion is set to 0.
        ///
        /// @since 3.0.1
        /// @jira_ticket CSHARP-388
        /// @expected_result MaxProtocolVersion is set and honored upon connection.
        ///
        /// @test_category connection
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Cluster_Should_Honor_MaxProtocolVersion_Set(bool asyncConnect)
        {
            _testCluster = TestClusterManager.CreateNew(2);

            // Default MaxProtocolVersion
            var clusterDefault = Cluster.Builder()
                .AddContactPoint(_testCluster.InitialContactPoint)
                .Build();
            Assert.AreEqual(Cluster.MaxProtocolVersion, clusterDefault.Configuration.ProtocolOptions.MaxProtocolVersion);

            // MaxProtocolVersion set
            var clusterMax = Cluster.Builder()
                .AddContactPoint(_testCluster.InitialContactPoint)
                .WithMaxProtocolVersion(3)
                .Build();
            Assert.AreEqual(3, clusterMax.Configuration.ProtocolOptions.MaxProtocolVersion);
            await Run(clusterMax, asyncConnect, session =>
            {
                if (CassandraVersion < Version.Parse("2.1"))
                    Assert.AreEqual(2, session.BinaryProtocolVersion);
                else
                    Assert.AreEqual(3, session.BinaryProtocolVersion);
            });
            
            // Arbitary MaxProtocolVersion set, will negotiate down upon connect
            var clusterNegotiate = Cluster.Builder()
                .AddContactPoint(_testCluster.InitialContactPoint)
                .WithMaxProtocolVersion(10)
                .Build();
            Assert.AreEqual(10, clusterNegotiate.Configuration.ProtocolOptions.MaxProtocolVersion);
            await Run(clusterNegotiate, asyncConnect, session =>
            {
                Assert.LessOrEqual(4, clusterNegotiate.Configuration.ProtocolOptions.MaxProtocolVersion);
            });

            // ProtocolVersion 0 does not exist
            Assert.Throws<ArgumentException>(
                () => Cluster.Builder().AddContactPoint("127.0.0.1").WithMaxProtocolVersion((byte)0));
        }


        /// <summary>
        /// Validates that the client adds the newly bootstrapped node and eventually queries from it
        /// </summary>
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Add_And_Query_Newly_Bootstrapped_Node(bool asyncConnect)
        {
            _testCluster = TestClusterManager.CreateNew();
            var cluster = Cluster.Builder().AddContactPoint(_testCluster.InitialContactPoint).Build();
            await Run(cluster, asyncConnect, session =>
            {
                Assert.AreEqual(1, cluster.AllHosts().Count);
                _testCluster.BootstrapNode(2);
                var queried = false;
                Trace.TraceInformation("Node bootstrapped");
                Thread.Sleep(10000);
                var newNodeAddress = _testCluster.ClusterIpPrefix + 2;
                Assert.True(TestUtils.IsNodeReachable(IPAddress.Parse(newNodeAddress)));
                //New node should be part of the metadata
                Assert.AreEqual(2, cluster.AllHosts().Count);
                for (var i = 0; i < 10; i++)
                {
                    var rs = session.Execute("SELECT key FROM system.local");
                    if (rs.Info.QueriedHost.Address.ToString() == newNodeAddress)
                    {
                        queried = true;
                        break;
                    }
                }
                Assert.True(queried, "Newly bootstrapped node should be queried");
            });
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Remove_Decommissioned_Node(bool asyncConnect)
        {
            const int numberOfNodes = 2;
            _testCluster = TestClusterManager.CreateNew(numberOfNodes);
            var cluster = Cluster.Builder()
                .AddContactPoints(_testCluster.InitialContactPoint, _testCluster.ClusterIpPrefix + numberOfNodes)
                .Build();
            await Run(cluster, asyncConnect, session =>
            {
                Assert.AreEqual(numberOfNodes, cluster.AllHosts().Count);
                if (TestClusterManager.DseVersion >= Version.Parse("5.1.0"))
                {
                    _testCluster.DecommissionNodeForcefully(numberOfNodes);
                }
                else
                {
                    _testCluster.DecommissionNode(numberOfNodes);
                }
                Trace.TraceInformation("Node decommissioned");
                Thread.Sleep(10000);
                var decommisionedNode = _testCluster.ClusterIpPrefix + numberOfNodes;
                Assert.False(TestUtils.IsNodeReachable(IPAddress.Parse(decommisionedNode)));
                //New node should be part of the metadata
                Assert.AreEqual(numberOfNodes - 1, cluster.AllHosts().Count);
                var queried = false;
                for (var i = 0; i < 10; i++)
                {
                    var rs = session.Execute("SELECT key FROM system.local");
                    if (rs.Info.QueriedHost.Address.ToString() == decommisionedNode)
                    {
                        queried = true;
                        break;
                    }
                }
                Assert.False(queried, "Removed node should be queried");
            });
        }

        private class TestLoadBalancingPolicy : ILoadBalancingPolicy
        {
            private ICluster _cluster;
            public Host ControlConnectionHost { get; private set; }

            public void Initialize(ICluster cluster)
            {
                _cluster = cluster;
                ControlConnectionHost = ((Cluster)cluster).GetControlConnection().Host;
            }

            public HostDistance Distance(Host host)
            {
                return HostDistance.Local;
            }

            public IEnumerable<Host> NewQueryPlan(string keyspace, IStatement query)
            {
                return _cluster.AllHosts();
            }
        }

        private static async Task Run(Cluster cluster, bool asyncConnect, Action<ISession> action)
        {
            if (asyncConnect)
            {
                try
                {
                    var session = await cluster.ConnectAsync();
                    action(session);
                }
                finally
                {
                    cluster?.ShutdownAsync().Wait();
                }
            }
            else
            {
                using (cluster)
                {
                    var session = cluster.Connect();
                    action(session);
                }
            }
        }
    }
}
