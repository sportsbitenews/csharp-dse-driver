//
//  Copyright (C) 2017 DataStax, Inc.
//
//  Please see the license for details:
//  http://www.datastax.com/terms/datastax-dse-driver-license-terms
//

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
﻿using System.Threading;
﻿using System.Threading.Tasks;
﻿using Dse.Requests;
 using Dse.Responses;
 using Dse.Tasks;
﻿using Microsoft.IO;

namespace Dse
{
    /// <summary>
    /// Represents the state of the ongoing operation for the Connection
    /// </summary>
    internal class OperationState
    {
        private const int StateInit = 0;
        private const int StateCancelled = 1;
        private const int StateTimedout = 2;
        private const int StateCompleted = 3;
        private Action<Exception, Response> _callback;
        public static readonly Action<Exception, Response> Noop = (_, __) => { };
        private volatile bool _timeoutCallbackSet;
        private int _state = StateInit;
        private volatile HashedWheelTimer.ITimeout _timeout;

        /// <summary>
        /// 8 byte header of the frame
        /// </summary>
        public FrameHeader Header { get; set; }

        public IRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the timeout in milliseconds for the request.
        /// </summary>
        public int TimeoutMillis { get; set; }

        /// <summary>
        /// Creates a new operation state with the provided callback
        /// </summary>
        public OperationState(Action<Exception, Response> callback)
        {
            Interlocked.Exchange(ref _callback, callback);
        }

        /// <summary>
        /// Sets the read timeout associated with the request
        /// </summary>
        public void SetTimeout(HashedWheelTimer.ITimeout value)
        {
            _timeout = value;
        }

        /// <summary>
        /// Marks this operation as completed and returns the callback.
        /// Note that the returned callback might be a reference to <see cref="Noop"/>, as the original callback
        /// might be already called.
        /// </summary>
        public Action<Exception, Response> SetCompleted()
        {
            var previousState = Interlocked.CompareExchange(ref _state, StateCompleted, StateInit);
            if (previousState == StateCancelled || previousState == StateCompleted)
            {
                return Noop;
            }
            Action<Exception, Response> callback;
            if (previousState == StateInit)
            {
                callback = Interlocked.Exchange(ref _callback, Noop);
                var timeout = _timeout;
                if (timeout != null)
                {
                    //Cancel it if it hasn't expired
                    timeout.Cancel();
                }
                return callback;
            }
            //Operation has timed out
            var spin = new SpinWait();
            while (!_timeoutCallbackSet)
            {
                //Wait for the timeout callback to be set
                spin.SpinOnce();
            }
            callback = Interlocked.Exchange(ref _callback, Noop);
            return callback;
        }

        /// <summary>
        /// Marks this operation as completed and invokes the callback with the exception using the default task scheduler.
        /// Its safe to call this method multiple times as the underlying callback will be invoked just once.
        /// </summary>
        public void InvokeCallback(Exception ex)
        {
            var callback = SetCompleted();
            if (callback == Noop)
            {
                return;
            }
            //Invoke the callback in a new thread in the thread pool
            //This way we don't let the user block on a thread used by the Connection
            Task.Factory.StartNew(() => callback(ex, null), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Marks this operation as timed-out, callbacks with the exception 
        /// and sets a handler when the response is received
        /// </summary>
        public bool MarkAsTimedOut(OperationTimedOutException ex, Action onReceive)
        {
            var previousState = Interlocked.CompareExchange(ref _state, StateTimedout, StateInit);
            if (previousState != StateInit)
            {
                return false;
            }
            //When the data is received, invoke on receive callback
            var callback = Interlocked.Exchange(ref _callback, (_, __) => onReceive());
#if !NETCORE
            Thread.MemoryBarrier();
#else
            Interlocked.MemoryBarrier();
#endif
            _timeoutCallbackSet = true;
            Task.Factory.StartNew(() => callback(ex, null), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            return true;
        }

        /// <summary>
        /// Removes the context associated with this request, if possible
        /// </summary>
        public void Cancel()
        {
            if (Interlocked.CompareExchange(ref _state, StateCancelled, StateInit) != StateInit)
            {
                return;
            }
            //Remove the closure
            Interlocked.Exchange(ref _callback, Noop);
            var timeout = _timeout;
            if (timeout != null)
            {
                //Cancel it if it hasn't expired
                //We should not worry about yielding OperationTimedOutExceptions when this is cancelled.
                timeout.Cancel();
            }
        }

        /// <summary>
        /// Asynchronously marks the provided operations as completed and invoke the callbacks with the exception.
        /// </summary>
        internal static void CallbackMultiple(IEnumerable<OperationState> ops, Exception ex)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var state in ops)
                {
                    var callback = state.SetCompleted();
                    callback(ex, null);
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
    }
}
