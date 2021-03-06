//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Threading;

namespace Dse
{
    /// <summary>
    ///  Additional options of the .net Cassandra driver.
    /// </summary>
    public class ClientOptions
    {
        private readonly string _defaultKeyspace;
        private readonly int _queryAbortTimeout = 60000;
        private readonly bool _withoutRowSetBuffering;

        public bool WithoutRowSetBuffering
        {
            get { return _withoutRowSetBuffering; }
        }

        /// <summary>
        /// Gets the query abort timeout for synchronous operations in milliseconds.
        /// </summary>
        public int QueryAbortTimeout
        {
            get { return _queryAbortTimeout; }
        }

        /// <summary>
        /// Gets the keyspace to be used after connecting to the cluster.
        /// </summary>
        public string DefaultKeyspace
        {
            get { return _defaultKeyspace; }
        }

        public ClientOptions()
        {
        }

        public ClientOptions(bool withoutRowSetBuffering, int queryAbortTimeout, string defaultKeyspace)
        {
            _withoutRowSetBuffering = withoutRowSetBuffering;
            _queryAbortTimeout = queryAbortTimeout;
            _defaultKeyspace = defaultKeyspace;
        }

        /// <summary>
        /// Returns the timeout in milliseconds based on the amount of queries.
        /// </summary>
        internal int GetQueryAbortTimeout(int amountOfQueries)
        {
            if (amountOfQueries <= 0)
            {
                throw new ArgumentException("The amount of queries must be a positive number");
            }
            if (_queryAbortTimeout == Timeout.Infinite)
            {
                return _queryAbortTimeout;
            }
            return _queryAbortTimeout*amountOfQueries;
        }
    }
}
