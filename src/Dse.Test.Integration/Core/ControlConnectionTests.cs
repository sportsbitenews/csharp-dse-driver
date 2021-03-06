//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using Dse.Test.Integration.TestClusterManagement;

namespace Dse.Test.Integration.Core
{
    [Category("short")]
    public class ControlConnectionTests : TestGlobals
    {
        private const int InitTimeout = 2000;
        private ITestCluster _testCluster;

        [OneTimeSetUp]
        public void SetupFixture()
        {
            _testCluster = TestClusterManager.CreateNew();
        }

        [Test]
        public void Should_Use_Maximum_Protocol_Version_Supported()
        {
            var cc = NewInstance();
            cc.Init().Wait(InitTimeout);
            Assert.AreEqual(GetExpectedProtocolVersion(), cc.ProtocolVersion);
            cc.Dispose();
        }

        [Test, TestCassandraVersion(2, 0)]
        public void Should_Use_Maximum_Protocol_Version_Provided()
        {
            var version = ProtocolVersion.V2;
            if (CassandraVersion >= Version.Parse("3.0"))
            {
                //protocol 2 is not supported in Cassandra 3.0+
                version = ProtocolVersion.V3;
            }
            var cc = NewInstance(version);
            cc.Init().Wait(InitTimeout);
            Assert.AreEqual(version, cc.ProtocolVersion);
            cc.Dispose();
        }

        [Test, TestCassandraVersion(2, 2, Comparison.LessThan)]
        public void Should_Downgrade_The_Protocol_Version()
        {
            //Use a higher protocol version
            var version = (ProtocolVersion)(GetExpectedProtocolVersion() + 1);
            var cc = NewInstance(version);
            cc.Init().Wait(InitTimeout);
            Assert.AreEqual(version - 1, cc.ProtocolVersion);
        }

        [Test]
        public void Should_Downgrade_The_Protocol_Version_With_Higher_Version_Than_Supported()
        {
            // Use a non-existent higher protocol version
            var version = (ProtocolVersion)0x0f;
            var cc = NewInstance(version);
            cc.Init().Wait(InitTimeout);
            Assert.AreEqual(GetExpectedProtocolVersion(), cc.ProtocolVersion);
        }

        private ControlConnection NewInstance(
            ProtocolVersion version = ProtocolVersion.MaxSupported, 
            Configuration config = null, 
            Metadata metadata = null)
        {
            config = config ?? new Configuration();
            if (metadata == null)
            {
                metadata = new Metadata(config);
                metadata.AddHost(new IPEndPoint(IPAddress.Parse(_testCluster.InitialContactPoint), ProtocolOptions.DefaultPort));
            }
            var cc = new ControlConnection(version, config, metadata);
            metadata.ControlConnection = cc;
            return cc;
        }

        private ProtocolVersion GetExpectedProtocolVersion()
        {
            var expectedVersion = ProtocolVersion.MaxSupported;
            if (TestClusterManager.CassandraVersion < Version.Parse("2.2"))
            {
                expectedVersion = ProtocolVersion.V3;
            }
            if (TestClusterManager.CassandraVersion < Version.Parse("2.1"))
            {
                expectedVersion = ProtocolVersion.V2;
            }
            if (TestClusterManager.CassandraVersion < Version.Parse("2.0"))
            {
                expectedVersion = ProtocolVersion.V1;
            }
            return expectedVersion;
        }
    }
}
