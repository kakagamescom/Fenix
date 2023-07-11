﻿using System;
using System.Linq;

namespace Module.Shared
{
    public static class DelegateExtensions {

        public static bool InvokeIfNotNull(this Action a) {
            if (a == null) { return false; } else { a.Invoke(); return true; }
        }
        public static bool InvokeIfNotNull<T>(this Action<T> a, T t) {
            if (a == null) { return false; } else { a.Invoke(t); return true; }
        }
        public static bool InvokeIfNotNull<T, V>(this Action<T, V> a, T t, V v) {
            if (a == null) { return false; } else { a.Invoke(t, v); return true; }
        }
        public static bool InvokeIfNotNull<T, U, V>(this Action<T, U, V> a, T t, U u, V v) {
            if (a == null) { return false; } else { a.Invoke(t, u, v); return true; }
        }

        public static object DynamicInvokeV2(this Delegate self, params object[] passedParams) {
            object result;
            DynamicInvokeV2(self, passedParams, out result, true);
            return result;
        }

        public static bool DynamicInvokeV2(this Delegate self, object[] passedParams, out object result, bool throwIfNotEnoughParams) {
            var methodParams = self.Method.GetParameters();
            if (methodParams.Length == passedParams.Length) {
                result = (self.DynamicInvoke(passedParams));
                return true;
            } else if (methodParams.Length < passedParams.Length) {
                var subset = passedParams.Take(methodParams.Length).ToArray();
                result = (self.DynamicInvoke(subset));
                return true;
            } else {
                var error = "Not enough parameters passed: " + self;
                if (throwIfNotEnoughParams) { throw new ArgumentException(error); } else { Log.Warning("Listener skipped: " + error); }
            }
            result = null;
            return false;
        }

        /// <summary>
        /// This will create an Action where the first call is executed and the last call is executed but 
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        /// <param name="skipFirstEvent"> if set to true there will be no instant execution of the very first call to the debounced async func </param>
        public static Action<S> AsThrottledDebounce<S>(this Action<S> self, double delayInMs, bool skipFirstEvent = false) {
            EventHandler<S> onChangedHandler = (_, value) => self(value);
            EventHandler<S> debounced = onChangedHandler.AsThrottledDebounce(delayInMs, skipFirstEvent);
            return (value) => debounced(null, value);
        }

        /// <summary>
        /// This will create an Action where the first call is executed and the last call is executed but 
        /// every call in between that is below the passed millisecond threshold is ignored
        /// </summary>
        /// <param name="skipFirstEvent"> if set to true there will be no instant execution of the very first call to the debounced async func </param>
        public static Action AsThrottledDebounce(this Action self, double delayInMs, bool skipFirstEvent = false) {
            EventHandler<object> onChangedHandler = (_, args) => self();
            EventHandler<object> debounced = onChangedHandler.AsThrottledDebounce(delayInMs, skipFirstEvent);
            return () => debounced(null, EventArgs.Empty);
        }

        
    }
}