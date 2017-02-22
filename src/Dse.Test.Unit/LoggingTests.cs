//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Dse.Test.Unit
{
    [TestFixture]
    public class LoggingTests
    {
        [Test]
        public void FactoryBasedLoggerHandler_Methods_Not_Throw()
        {
            UseAllMethods(new Logger.FactoryBasedLoggerHandler(typeof(int)));
        }

        [Test]
        public void FactoryBasedLoggerHandler_Methods_Should_Output_To_Trace()
        {
            var originalLevel = Diagnostics.CassandraTraceSwitch.Level;
            Diagnostics.CassandraTraceSwitch.Level = TraceLevel.Verbose;
            var listener = new TestTraceListener();
            Trace.Listeners.Add(listener);
            var loggerHandler = new Logger.TraceBasedLoggerHandler(typeof(int));
            UseAllMethods(loggerHandler);
            Trace.Listeners.Remove(listener);
            Diagnostics.CassandraTraceSwitch.Level = originalLevel;
            Assert.AreEqual(6, listener.Messages.Count);
            var expectedMessages = new[]
            {
                "Test exception 1",
                "Message 1",
                "Message 2 Param1",
                "Message 3 Param2",
                "Message 4 Param3",
                "Message 5 Param4"
            };
            var messages = listener.Messages.Keys.OrderBy(k => k).Select(k => listener.Messages[k]).ToArray();
            for (var i = 0; i < expectedMessages.Length; i++)
            {
                StringAssert.Contains(expectedMessages[i], messages[i]);
            }
        }

        [Test]
        public void FactoryBasedLoggerHandler_LogError_Handles_Concurrent_Calls()
        {
            var originalLevel = Diagnostics.CassandraTraceSwitch.Level;
            try
            {
                Diagnostics.CassandraTraceSwitch.Level = TraceLevel.Verbose;
                var listener = new TestTraceListener();
                Trace.Listeners.Add(listener);
                var loggerHandler = new Logger.TraceBasedLoggerHandler(typeof(int));
                UseAllMethods(loggerHandler);
                Trace.Listeners.Remove(listener);
                Assert.AreEqual(6, listener.Messages.Count);
                var actions = Enumerable
                    .Repeat(true, 1000)
                    .Select<bool, Action>((_, index) => () =>
                    {
                        loggerHandler.Error(new ArgumentException("Test exception " + index,
                            new Exception("Test inner exception")));
                    });
                TestHelper.ParallelInvoke(actions);
            }
            finally
            {
                Diagnostics.CassandraTraceSwitch.Level = originalLevel;
            }
        }

        private void UseAllMethods(Logger.ILoggerHandler loggerHandler)
        {
            loggerHandler.Error(new Exception("Test exception 1"));
            loggerHandler.Error("Message 1", new Exception("Test exception 1"));
            loggerHandler.Error("Message 2 {0}", "Param1");
            loggerHandler.Info("Message 3 {0}", "Param2");
            loggerHandler.Verbose("Message 4 {0}", "Param3");
            loggerHandler.Warning("Message 5 {0}", "Param4");
        }

        private class TestTraceListener : TraceListener
        {
            public readonly ConcurrentDictionary<int, string> Messages = new ConcurrentDictionary<int, string>();
            private int _counter = -1;

            public override void Write(string message)
            {
            }

            public override void WriteLine(string message)
            {
                Messages.AddOrUpdate(Interlocked.Increment(ref _counter), message, (k, v) => message);
            }
        }
    }
}