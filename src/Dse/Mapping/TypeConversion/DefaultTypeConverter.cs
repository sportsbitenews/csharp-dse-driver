//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;

namespace Dse.Mapping.TypeConversion
{
    /// <summary>
    /// A default implementation of TypeConversionFactory that doesn't do any user defined conversions.
    /// </summary>
    public class DefaultTypeConverter : TypeConverter
    {
        /// <summary>
        /// Always returns null.
        /// </summary>
        protected override Func<TDatabase, TPoco> GetUserDefinedFromDbConverter<TDatabase, TPoco>()
        {
            return null;
        }

        /// <summary>
        /// Always returns null.
        /// </summary>
        protected override Func<TPoco, TDatabase> GetUserDefinedToDbConverter<TPoco, TDatabase>()
        {
            return null;
        }
    }
}