//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System.Net;

namespace Dse
{
    internal class TopologyChangeEventArgs : CassandraEventArgs
    {
        public enum Reason
        {
            NewNode,
            RemovedNode
        };

        public IPEndPoint Address;
        public Reason What;
    }
}