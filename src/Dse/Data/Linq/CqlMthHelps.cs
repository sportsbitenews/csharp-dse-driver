//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System.Reflection;

namespace Dse.Data.Linq
{
    internal class CqlMthHelps
    {
        internal static MethodInfo SelectMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("Select", BindingFlags.NonPublic | BindingFlags.Static);
        internal static MethodInfo WhereMi = typeof(CqlMthHelps).GetTypeInfo().GetMethod("Where", BindingFlags.NonPublic | BindingFlags.Static);
        internal static MethodInfo GroupByMi = typeof(CqlMthHelps).GetTypeInfo().GetMethod("GroupBy", BindingFlags.NonPublic | BindingFlags.Static);
        internal static MethodInfo UpdateIfMi = typeof(CqlMthHelps).GetTypeInfo().GetMethod("UpdateIf", BindingFlags.NonPublic | BindingFlags.Static);
        internal static MethodInfo DeleteIfMi = typeof(CqlMthHelps).GetTypeInfo().GetMethod("DeleteIf", BindingFlags.NonPublic | BindingFlags.Static);
        internal static MethodInfo FirstMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("First", BindingFlags.NonPublic | BindingFlags.Static);

        internal static MethodInfo First_ForCQLTableMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("First",
                                                                                        new[] {typeof (ITable), typeof (int), typeof (object)});

        internal static MethodInfo FirstOrDefaultMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("FirstOrDefault", BindingFlags.NonPublic | BindingFlags.Static);

        internal static MethodInfo FirstOrDefault_ForCQLTableMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("FirstOrDefault",
                                                                                                 new[]
                                                                                                 {typeof (ITable), typeof (int), typeof (object)});

        internal static MethodInfo TakeMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("Take", BindingFlags.NonPublic | BindingFlags.Static);
        internal static MethodInfo AllowFilteringMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("AllowFiltering", BindingFlags.NonPublic | BindingFlags.Static);
        internal static MethodInfo CountMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("Count", BindingFlags.NonPublic | BindingFlags.Static);
        internal static MethodInfo OrderByMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("OrderBy", BindingFlags.NonPublic | BindingFlags.Static);

        internal static MethodInfo OrderByDescendingMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("OrderByDescending",
                                                                                        BindingFlags.NonPublic | BindingFlags.Static);

        internal static MethodInfo ThenByMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("ThenBy", BindingFlags.NonPublic | BindingFlags.Static);

        internal static MethodInfo ThenByDescendingMi = typeof (CqlMthHelps).GetTypeInfo().GetMethod("ThenByDescending",
                                                                                       BindingFlags.NonPublic | BindingFlags.Static);

        internal static object Select(object a, object b)
        {
            return null;
        }

        internal static object Where(object a, object b)
        {
            return null;
        }

        internal static object UpdateIf(object a, object b)
        {
            return null;
        }

        internal static object DeleteIf(object a, object b)
        {
            return null;
        }

        internal static object First(object a, int b)
        {
            return null;
        }

        internal static object FirstOrDefault(object a, int b)
        {
            return null;
        }

        internal static object Take(object a, int b)
        {
            return null;
        }

        internal static object AllowFiltering(object a)
        {
            return null;
        }

        internal static object Count(object a)
        {
            return null;
        }

        internal static object OrderBy(object a, object b)
        {
            return null;
        }

        internal static object GroupBy(object a, object b)
        {
            return null;
        }

        internal static object OrderByDescending(object a, object b)
        {
            return null;
        }

        internal static object ThenBy(object a, object b)
        {
            return null;
        }

        internal static object ThenByDescending(object a, object b)
        {
            return null;
        }

        public static object First(ITable a, int b, object c)
        {
            return null;
        }

        public static object FirstOrDefault(ITable a, int b, object c)
        {
            return null;
        }
    }
}