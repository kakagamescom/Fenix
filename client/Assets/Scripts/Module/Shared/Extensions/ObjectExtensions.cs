using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Module.Shared {

    public static class ObjectExtensions {

        public static void ThrowErrorIfNull(this object self, string paramName, [CallerMemberName] string methodName = null) {
            if (self == null) {
                System.Diagnostics.Debugger.Break();
                throw new ArgumentNullException($"{paramName} (Method {methodName})");
            }
        }

        public static void ThrowErrorIfNull(this object self, Func<Exception> onNull) {
            if (self == null) {
                System.Diagnostics.Debugger.Break();
                throw onNull();
            }
        }

        public static async Task ThrowErrorIfNull(this object self, Func<Task<Exception>> onNull) {
            if (self == null) {
                System.Diagnostics.Debugger.Break();
                throw await onNull();
            }
        }

    }

}