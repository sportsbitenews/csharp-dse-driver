//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

namespace Dse
{
    /// <summary>
    ///  Indicates a syntax error in a query.
    /// </summary>
    public class SyntaxError : QueryValidationException
    {
        public SyntaxError(string message) : base(message)
        {
        }
    }
}
