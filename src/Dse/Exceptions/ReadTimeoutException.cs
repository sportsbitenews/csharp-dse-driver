//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

namespace Dse
{
    /// <summary>
    ///  A Cassandra timeout during a read query.
    /// </summary>
    public class ReadTimeoutException : QueryTimeoutException
    {
        public bool WasDataRetrieved { get; private set; }

        public ReadTimeoutException(ConsistencyLevel consistency, int received, int required,
                                    bool dataPresent) :
                                        base(
                                        string.Format("Cassandra timeout during read query at consistency {0} ({1})",
                                                      consistency, FormatDetails(received, required, dataPresent)),
                                        consistency,
                                        received,
                                        required)
        {
            WasDataRetrieved = dataPresent;
        }

        private static string FormatDetails(int received, int required, bool dataPresent)
        {
            if (received < required)
                return string.Format("{0} replica(s) responded over {1} required", received, required);
            if (!dataPresent)
                return string.Format("the replica queried for data didn't respond");
            return string.Format("timeout while waiting for repair of inconsistent replica");
        }
    }
}
