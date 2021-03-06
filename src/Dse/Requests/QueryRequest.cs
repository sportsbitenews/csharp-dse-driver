//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using System.IO;
using Dse.Serialization;

namespace Dse.Requests
{
    /// <summary>
    /// Represents a protocol QUERY request
    /// </summary>
    internal class QueryRequest : IQueryRequest, ICqlRequest
    {
        public const byte OpCode = 0x07;

        public ConsistencyLevel Consistency
        {
            get { return _queryOptions.Consistency; }
            set { _queryOptions.Consistency = value;}
        }

        public byte[] PagingState
        {
            get { return _queryOptions.PagingState; }
            set { _queryOptions.PagingState = value; }
        }

        public int PageSize
        {
            get { return _queryOptions.PageSize; }
        }

        public ConsistencyLevel SerialConsistency
        {
            get { return _queryOptions.SerialConsistency; }
        }

        public string Query { get { return _cqlQuery; }}

        public IDictionary<string, byte[]> Payload { get; set; }

        private readonly string _cqlQuery;
        private FrameHeader.HeaderFlag _headerFlags;
        private readonly QueryProtocolOptions _queryOptions;

        public QueryRequest(ProtocolVersion protocolVersion, string cqlQuery, bool tracingEnabled, QueryProtocolOptions queryOptions)
        {
            _cqlQuery = cqlQuery;
            _queryOptions = queryOptions;
            if (tracingEnabled)
            {
                _headerFlags = FrameHeader.HeaderFlag.Tracing;
            }
            if (queryOptions == null)
            {
                throw new ArgumentNullException("queryOptions");
            }
            if (queryOptions.SerialConsistency != ConsistencyLevel.Any && queryOptions.SerialConsistency.IsSerialConsistencyLevel() == false)
            {
                throw new RequestInvalidException("Non-serial consistency specified as a serial one.");
            }
            if (!protocolVersion.SupportsTimestamp())
            {
                //Features supported in protocol v3 and above
                if (queryOptions.Timestamp != null)
                {
                    throw new NotSupportedException("Timestamp for query is supported in Cassandra 2.1 and above.");
                }
                if (queryOptions.ValueNames != null && queryOptions.ValueNames.Count > 0)
                {
                    throw new NotSupportedException("Query parameter names feature is supported in Cassandra 2.1 and above.");
                }
            }
        }

        public int WriteFrame(short streamId, MemoryStream stream, Serializer serializer)
        {
            var wb = new FrameWriter(stream, serializer);
            if (Payload != null)
            {
                _headerFlags |= FrameHeader.HeaderFlag.CustomPayload;
            }
            wb.WriteFrameHeader((byte)_headerFlags, streamId, OpCode);
            if (Payload != null)
            {
                //A custom payload for this request
                wb.WriteBytesMap(Payload);
            }
            wb.WriteLongString(_cqlQuery);
            _queryOptions.Write(wb, false);
            return wb.Close();
        }

        public void WriteToBatch(FrameWriter wb)
        {
            //not a prepared query
            wb.WriteByte(0);
            wb.WriteLongString(_cqlQuery);
            if (_queryOptions.Values == null || _queryOptions.Values.Length == 0)
            {
                //not values
                wb.WriteInt16(0);
            }
            else
            {
                wb.WriteUInt16((ushort) _queryOptions.Values.Length);
                foreach (var queryParameter in _queryOptions.Values)
                {
                    wb.WriteAsBytes(queryParameter);
                }
            }
        }
    }
}
