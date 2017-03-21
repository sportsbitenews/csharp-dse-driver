//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

namespace Dse.Data.Linq
{
    internal enum ParsePhase
    {
        None,
        Select,
        What,
        Condition,
        SelectBinding,
        Take,
        OrderBy,
        OrderByDescending
    };
}