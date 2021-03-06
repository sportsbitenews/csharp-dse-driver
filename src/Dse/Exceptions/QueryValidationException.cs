//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;

namespace Dse
{
    /// <summary>
    ///  An exception indicating that a query cannot be executed because it is
    ///  incorrect syntactically, invalid, unauthorized or any other reason.
    /// </summary>
    public abstract class QueryValidationException : DriverException
    {
        public QueryValidationException(string message)
            : base(message)
        {
        }

        public QueryValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
