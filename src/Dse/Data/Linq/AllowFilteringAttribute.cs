//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;

namespace Dse.Data.Linq
{
    /// <summary>
    /// The ALLOW FILTERING option allows to explicitly allow queries that require filtering. 
    /// Please note that a query using ALLOW FILTERING may thus have unpredictable performance (for the definition above), i.e. even a query that selects a handful of records may exhibit performance that depends on the total amount of data stored in the cluster.
    /// </summary>
    [Obsolete("Linq attributes are deprecated, use mapping attributes defined in Dse.Mapping.Attributes instead.")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class AllowFilteringAttribute : Attribute
    {
    }
}