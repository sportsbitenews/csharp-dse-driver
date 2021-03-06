//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

namespace Dse.Responses
{
    internal class ReadyResponse : Response
    {
        public const byte OpCode = 0x02;

        internal ReadyResponse(Frame frame)
            : base(frame)
        {
        }

        internal static ReadyResponse Create(Frame frame)
        {
            return new ReadyResponse(frame);
        }
    }
}
