//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Dse.Test.Unit
{
    [TestFixture]
    public class HostTests
    {
        private static readonly IPEndPoint Address = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1000);

        [Test]
        public void BringUpIfDown_Should_Allow_Multiple_Concurrent_Calls()
        {
            var host = new Host(Address);
            var counter = 0;
            host.Up += _ => Interlocked.Increment(ref counter);
            host.SetDown();
            TestHelper.ParallelInvoke(() =>
            {
                host.BringUpIfDown();
            }, 100);
            //Should fire event only once
            Assert.AreEqual(1, counter);
        }
    }
}
