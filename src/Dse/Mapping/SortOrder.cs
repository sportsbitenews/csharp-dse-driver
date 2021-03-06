//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//


namespace Dse.Mapping
{
    /// <summary>
    /// Specifies sort order
    /// </summary>
    public enum SortOrder: sbyte
    {
        Unspecified = 0,
        Ascending = 1,
        Descending = -1,
    }
}
