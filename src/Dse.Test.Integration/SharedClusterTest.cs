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
using System.Text;
using Dse.Test.Integration.TestClusterManagement;
using NUnit.Framework;

namespace Dse.Test.Integration
{
    /// <summary>
    /// Represents a test fixture that on setup, it creates a test cluster available for all tests.
    /// <para>
    /// With a shared session and cluster.
    /// </para>
    /// </summary>
    [TestFixture]
    public abstract class SharedClusterTest : TestGlobals
    {
        private static ITestCluster _reusableInstance;
        private readonly bool _reuse;
        protected readonly List<ICluster> ClusterInstances = new List<ICluster>();

        /// <summary>
        /// Gets the amount of nodes in the test cluster
        /// </summary>
        protected int AmountOfNodes { get; private set; }

        /// <summary>
        /// Determines if an ISession needs to be created to share during the lifetime of this instance
        /// </summary>
        protected bool CreateSession { get; set; }

        /// <summary>
        /// Gets the Cassandra cluster that is used for testing
        /// </summary>
        protected ITestCluster TestCluster { get; private set; }

        /// <summary>
        /// The shared cluster instance of the fixture
        /// </summary>
        protected Cluster Cluster { get; private set; }

        /// <summary>
        /// The shared Session instance of the fixture
        /// </summary>
        protected ISession Session { get; set; }

        /// <summary>
        /// It executes the queries provided on test fixture setup.
        /// Ignored when null.
        /// </summary>
        protected virtual string[] SetupQueries
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the name of the default keyspace used for this instance
        /// </summary>
        protected string KeyspaceName { get; set; }

        protected SharedClusterTest(int amountOfNodes = 1, bool createSession = true, bool reuse = true)
        {
            //only reuse single node clusters
            _reuse = reuse && amountOfNodes == 1;
            AmountOfNodes = amountOfNodes;
            KeyspaceName = TestUtils.GetUniqueKeyspaceName().ToLowerInvariant();
            CreateSession = createSession;
        }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            if (_reuse && _reusableInstance != null && ReferenceEquals(_reusableInstance, TestClusterManager.LastInstance))
            {
                Trace.WriteLine("Reusing single node ccm instance");
                TestCluster = _reusableInstance;
            }
            else
            {
                TestCluster = TestClusterManager.CreateNew(AmountOfNodes);
                if (_reuse)
                {
                    _reusableInstance = TestCluster;
                }
                else
                {
                    _reusableInstance = null;
                }
            }
            if (CreateSession)
            {
                CreateCommonSession();
                if (SetupQueries != null)
                {
                    ExecuteSetupQueries();
                }
            }
        }

        protected virtual void CreateCommonSession()
        {
            Cluster = Cluster.Builder().AddContactPoint(TestCluster.InitialContactPoint)
                .WithQueryTimeout(60000)
                .WithSocketOptions(new SocketOptions().SetConnectTimeoutMillis(30000))
                .Build();
            Session = (Session)Cluster.Connect();
            Session.CreateKeyspace(KeyspaceName, null, false);
            Session.ChangeKeyspace(KeyspaceName);
        }

        protected virtual void ExecuteSetupQueries()
        {
            foreach (var query in SetupQueries)
            {
                Session.Execute(query);
            }
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            if (Cluster != null)
            {
                Cluster.Shutdown(1000);   
            }
            //Shutdown the other instances created by helper methods
            foreach (var c in ClusterInstances)
            {
                c.Shutdown(1000);
            }
        }

        protected ISession GetNewSession(string keyspace = null)
        {
            return GetNewCluster().Connect(keyspace);
        }

        protected ICluster GetNewCluster()
        {
            var cluster = Cluster.Builder().AddContactPoint(TestCluster.InitialContactPoint).Build();
            ClusterInstances.Add(cluster);
            return cluster;
        }
    }
}
