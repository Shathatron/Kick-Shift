using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

namespace Moball {
    public static class ArrayUtility {
        public static bool AllElementsEqual<T>(this IReadOnlyList<T> array) where T : IEquatable<T> {
            if (array.Count == 0) return true;
            var first = array[0];
            for (var i = 1; i < array.Count; i++) {
                if (!array[i].Equals(first)) return false;
            }
            return true;
        }
        public static T[] Cast<T>(this object[] array) {
            var result = new T[array.Length];
            for (int i = 0; i < array.Length; i++) {
                result[i] = (T) array[i];
            }
            return result;
        }

        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1)
            {
                int k = Random.Range(0, n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }

        public static bool Contains<T>(this T[] array, T element) {
            return Array.IndexOf(array, element) != -1;
        }

        public static void SwapElements<T>(T[] array, int from, int to) {
            T temp = array[to];
            array[to] = array[from];
            array[from] = temp;
        }

        public static T Sample<T>(this IReadOnlyList<T> list) {
            if (list.Count == 0) {
                    throw new System.InvalidOperationException("list.Count == 0");
            }
            return list[Random.Range(0, list.Count)];
        }

        public static T[] Slice<T>(T[] array, int from, int length) {
            var copy = new T[length];
            Array.Copy(array, from, copy, 0, length);
            return copy;
        }

        public static T[] CopyReverse<T>(T[] array) {
            var copy = new T[array.Length];
            Array.Copy(array, copy, array.Length);
            Array.Reverse(copy);
            return copy;
        }

        public static int[] Range(int first, int count) {
            var result = new int[count];
            for (int i = 0; i < count; i++) {
                result[i] = first + i;
            }
            return result;
        }

        public static int[] Range(int count) {
            return Range(0, count);
        }

        public static int GetWidth<T>(T[,] array) {
            return array.GetLength(0);
        }

        public static int GetHeight<T>(T[,] array) {
            return array.GetLength(1);
        }

        public static void Fill<T>(T[] array, T value) {
            for (int i = 0; i < array.Length; i++) {
                array[i] = value;
            }
        }

        public static void Fill<T>(T[,] array, T value) {
            var length0 = array.GetLength(0);
            var length1 = array.GetLength(1);
            for (int i = 0; i < length0; i++) {
                for (int j = 0; j < length1; j++) {
                    array[i, j] = value;
                }
            }
        }

        public static void Fill<T>(T[] array, System.Func<int, T> getValue) {
            for (int i = 0; i < array.Length; i++) {
                array[i] = getValue(i);
            }
        }

        public static T[] CreateFilled<T>(int length, T value) {
            var result = new T[length];
            Fill(result, value);
            return result;
        }

        public static T[,] CreateFilled<T>(int length0, int length1, T value) {
            var result = new T[length0, length1];
            Fill(result, value);
            return result;
        }

        public static T[] CreateFilled<T>(int length, System.Func<int, T> getValue) {
            var result = new T[length];
            Fill(result, getValue);
            return result;
        }

        public static T[,] CreateSquare<T>(int size) {
            return new T[size, size];
        }

        public static int MinIndex(int[] array) {
            int index = -1;
            int minValue = int.MaxValue;
            for (int i = 0; i < array.Length; i++) {
                if (array[i] < minValue) {
                    minValue = array[i];
                    index = i;
                }
            }
            return index;
        }

        public static int MinIndexBy<T, C>(this IReadOnlyList<T> array, Func<T, C> selector) where C : IComparable<C> {
            if (array.Count == 0) {
                throw new InvalidOperationException("Collection is empty");
            }
            int index = 0;
            C minValue = selector(array[0]);
            for (int i = 1; i < array.Count; i++) {
                var value = selector(array[i]);
                if (value.CompareTo(minValue) < 0) {
                    minValue = value;
                    index = i;
                }
            }
            return index;
        }

        public static int MaxIndex(int[] array) {
            int index = -1;
            int maxValue = int.MinValue;
            for (int i = 0; i < array.Length; i++) {
                if (array[i] > maxValue) {
                    maxValue = array[i];
                    index = i;
                }
            }
            return index;
        }

        public static T[] From<T>(T[] source) {
            var result = new T[source.Length];
            Array.Copy(source, result, source.Length);
            return result;
        }

        public static T[,] From<T>(T[,] source) {
            var result = new T[source.GetLength(0), source.GetLength(1)];
            Array.Copy(source, result, source.Length);
            return result;
        }

        /// Instead of `IEnumerable<T>.ToArray` which requires importing
        /// LINQ.
        public static T[] From<T>(IEnumerable<T> enumerable) {
            var result = new List<T>();
            foreach (var element in enumerable) {
                result.Add(element);
            }
            return result.ToArray();
        }

        // Adapted from:
        // https://stackoverflow.com/questions/4423318/how-to-compare-arrays-in-c#
        public static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++) {
                if (!a1[i].Equals(a2[i])) return false;
            }
            return true;
        }

        public static T[] SetArrayLength<T>(T[] array, int length, T filler = default(T))
        {
            if (array.Length == length) return array;

            var result = new T[length];
            var definedCount = Mathf.Min(array.Length, length);

            for (int i = 0; i < length; i++)
            {
                result[i] = i < definedCount
                        ? array[i]
                        : filler;
            }

            return result;
        }

        public static int Sum(int[] array) {
            return Sum(array, array.Length);
        }

        public static int Sum(int[] array, int length) {
            var result = 0;
            for (int i = 0; i < length; i++) {
                result += array[i];
            }
            return result;
        }
    }
}
