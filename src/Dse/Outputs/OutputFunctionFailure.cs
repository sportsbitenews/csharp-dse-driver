//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dse
{
    /// <summary>
    /// Parses a FunctionFailureException from a function failure error
    /// </summary>
    internal class OutputFunctionFailure : OutputError
    {
        private FunctionFailureException _exception;

        public override DriverException CreateException()
        {
            return _exception;
        }

        protected override void Load(FrameReader reader)
        {
            _exception = new FunctionFailureException(Message)
            {
                Keyspace = reader.ReadString(),
                Name = reader.ReadString(),
                ArgumentTypes = reader.ReadStringList()
            };
        }
    }
}
