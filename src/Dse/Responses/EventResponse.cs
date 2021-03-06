//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

namespace Dse.Responses
{
    internal class EventResponse : Response
    {
        public const byte OpCode = 0x0C;
        private readonly Logger _logger = new Logger(typeof (EventResponse));
        /// <summary>
        /// Information on the actual event
        /// </summary>
        public CassandraEventArgs CassandraEventArgs { get; set; }

        internal EventResponse(Frame frame)
            : base(frame)
        {
            string eventTypeString = Reader.ReadString();
            if (eventTypeString == "TOPOLOGY_CHANGE")
            {
                var ce = new TopologyChangeEventArgs();
                ce.What = Reader.ReadString() == "NEW_NODE"
                              ? TopologyChangeEventArgs.Reason.NewNode
                              : TopologyChangeEventArgs.Reason.RemovedNode;
                ce.Address = Reader.ReadInet();
                CassandraEventArgs = ce;
                return;
            }
            if (eventTypeString == "STATUS_CHANGE")
            {
                var ce = new StatusChangeEventArgs();
                ce.What = Reader.ReadString() == "UP"
                              ? StatusChangeEventArgs.Reason.Up
                              : StatusChangeEventArgs.Reason.Down;
                ce.Address = Reader.ReadInet();
                CassandraEventArgs = ce;
                return;
            }
            if (eventTypeString == "SCHEMA_CHANGE")
            {
                HandleSchemaChange(frame);
                return;
            }

            var ex = new DriverInternalError("Unknown Event Type");
            _logger.Error(ex);
            throw ex;
        }

        public void HandleSchemaChange(Frame frame)
        {
            var ce = new SchemaChangeEventArgs();
            CassandraEventArgs = ce;
            var changeTypeText = Reader.ReadString();
            SchemaChangeEventArgs.Reason changeType;
            switch (changeTypeText)
            {
                case "UPDATED":
                    changeType = SchemaChangeEventArgs.Reason.Updated;
                    break;
                case "DROPPED":
                    changeType = SchemaChangeEventArgs.Reason.Dropped;
                    break;
                default:
                    changeType = SchemaChangeEventArgs.Reason.Created;
                    break;
            }
            ce.What = changeType;
            if (!frame.Header.Version.SupportsSchemaChangeFullMetadata())
            {
                //protocol v1 and v2: <change_type><keyspace><table>
                ce.Keyspace = Reader.ReadString();
                ce.Table = Reader.ReadString();
                return;
            }
            //protocol v3+: <change_type><target><options>
            var target = Reader.ReadString();
            ce.Keyspace = Reader.ReadString();
            switch (target)
            {
                case "TABLE":
                    ce.Table = Reader.ReadString();
                    break;
                case "TYPE":
                    ce.Type = Reader.ReadString();
                    break;
                case "FUNCTION":
                    ce.FunctionName = Reader.ReadString();
                    ce.Signature = Reader.ReadStringList();
                    break;
                case "AGGREGATE":
                    ce.AggregateName = Reader.ReadString();
                    ce.Signature = Reader.ReadStringList();
                    break;
            }
        }

        internal static EventResponse Create(Frame frame)
        {
            return new EventResponse(frame);
        }
    }
}
