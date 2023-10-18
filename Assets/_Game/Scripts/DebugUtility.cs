using System.Diagnostics;
using System;

namespace Moball {
    public static class DebugUtility {
        [Conditional("UNITY_EDITOR")]
        public static void AssertNoMatch<T>(T value) {
            throw NoMatch(value);
        }

        [Conditional("UNITY_EDITOR")]
        public static void AssertNoMatch<T>(T value, string name) {
            throw NoMatch(value, name);
        }

        public static InvalidOperationException NoMatch<T>(T value) {
            return NoMatch(value, typeof(T).Name);
        }

        public static InvalidOperationException NoMatch<T>(T value, string name) {
            return new InvalidOperationException($"Unhandled {name} value: {value}");
        }
    }
}