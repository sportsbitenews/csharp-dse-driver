//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

namespace Dse
{
    internal class OutputOverloaded : OutputError
    {
        public override DriverException CreateException()
        {
            return new OverloadedException(Message);
        }

        protected override void Load(FrameReader reader)
        {
        }
    }
}