//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Runtime.Serialization;

namespace Dse
{
    /// <summary>
    /// Top level class for exceptions thrown by the driver.
    /// </summary>
#if !NETCORE
    [Serializable]
#endif
    public class DriverException : Exception
    {
        public DriverException(string message)
            : base(message)
        {
        }

        public DriverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

#if !NETCORE
        protected DriverException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            
        }
#endif    
    }
}
