//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

﻿using System;

namespace Dse
{
    public class RetryLoadBalancingPolicyEventArgs : EventArgs
    {
        public bool Cancel = false;
        public long DelayMs { get; private set; }

        public RetryLoadBalancingPolicyEventArgs(long delayMs)
        {
            DelayMs = delayMs;
        }
    }
}