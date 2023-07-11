﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Module.Shared
{
    public static class EventHandlerExtensions {

        public class EventHandlerResult<T> {
            public T result;
        }

        [Obsolete("Use Action instead, functions cant be debounced correctly in all cases since at the time of calling the function it cant be know if it should execute and return something or not", true)]
        public static Func<T, R> AsThrottledDebounce<T, R>(this Func<T, R> self, double delayInMs, bool skipFirstEvent = false) {
            EventHandler<EventHandlerResult<R>> action = (input, output) => { output.result = self((T)input); };
            EventHandler<EventHandlerResult<R>> throttledAction = action.AsThrottledDebounce(delayInMs, skipFirstEvent);
            return (T input) => {
                var output = new EventHandlerResult<R>();
                throttledAction(input, output);
                return output.result;
            };
        }

        /// <summary> 
        /// This will create an EventHandler where the first call is executed and the last call is executed but 
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        /// <param name="skipFirstEvent"> if set to true there will be no instant execution of the very first call to the debounced async func </param>
        public static EventHandler<T> AsThrottledDebounce<T>(this EventHandler<T> self, double delayInMs, bool skipFirstEvent = false) {
            int triggerFirstEvent = skipFirstEvent ? 0 : 1;
            int last = 0;
            return (sender, eventArgs) => {
                var current = Interlocked.Increment(ref last);
                if (!skipFirstEvent && ThreadSafety.FlipToFalse(ref triggerFirstEvent)) {
                    self(sender, eventArgs);
                } else {
                    Task.Delay((int)delayInMs).ContinueWith(task => {
                        if (current == last) {
                            self(sender, eventArgs);
                        }
                    });
                }
            };
        }

    }
}