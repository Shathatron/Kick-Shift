using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moball
{
	
    public static class Utils
	{
		public static void Swap<T>(ref T a, ref T b)
		{
			(a, b) = (b, a);
		}
		
		public static void SwapWith<T>(this ref T a, ref T b) where T : struct 
		{
			(a, b) = (b, a);
		}
		
		public static T ConvertToType<T>(object convertFrom)
		{
			T result = default;
			if(convertFrom is T from)
			{
				result = from;
			}
			else
			{
				Debug.LogError($"Cannot convert {convertFrom} to {typeof(T).Name}");
			}
			return result;
		}

		public static bool SetIfDifferent<T>(ref T setOnReference, T newValue) where T : class
		{
			if (setOnReference != newValue)
			{
				setOnReference = newValue;
				return true;
			}

			return false;
		}
		
		public static bool UpdateToBeWithinRange<T>(this ref T setOnReference, T minValue, T maxValue) where T : struct, IComparable<T>
		{
			if (setOnReference.CompareTo(minValue) < 0)
			{
				setOnReference = minValue;
				return true;
			}
			
			if (setOnReference.CompareTo(maxValue) > 0)
			{
				setOnReference = maxValue;
				return true;
			}

			return false;
		}

		public static bool UpdateMaxValue<T>(this ref T setOnReference, T newValue) where T : struct, IComparable<T>
		{
			if (setOnReference.CompareTo(newValue) < 0)
			{
				setOnReference = newValue;
				return true;
			}

			return false;
		}
		
		public static bool UpdateMinValue<T>(this ref T setOnReference, T newValue) where T : struct, IComparable<T>
		{
			if (setOnReference.CompareTo(newValue) > 0)
			{
				setOnReference = newValue;
				return true;
			}

			return false;
		}

		public static bool UpdateValue<T>(this ref T? setOnReference, T newValue) where T : struct, IEquatable<T>
		{
			if (setOnReference.HasValue == false || setOnReference.Value.Equals(newValue) == false)
			{
				setOnReference = newValue;
				return true;
			}

			return false;
		}

		public static int ToUnilateral(this (bool, bool) positiveAndNegative) =>
			positiveAndNegative.Item1 ? 1 : positiveAndNegative.Item2 ? -1 : 0;

		public static bool UpdateValue<T>(this ref T setOnReference, T newValue) where T : struct, IEquatable<T>
		{
			if (setOnReference.Equals(newValue) == false)
			{
				setOnReference = newValue;
				return true;
			}

			return false;
		}
		
		public static void DestroyObjectImmediate(this Object obj)
		{
			if (obj)
			{
				Object.DestroyImmediate(obj);
			}
		}
		
		public static void DestroyObjectSafe(this Object obj)
		{
			if (obj)
			{
				#if UNITY_EDITOR

				if (Application.IsPlaying(obj) == false)
				{
					Object.DestroyImmediate(obj);
					return;
				}
				
				#endif
				
				Object.Destroy(obj);
			}
		}
		
	}
	
	public static class DefaultRefValue<T>
	{
		private static T value = default;

		public static ref T Value
		{
			get
			{
				value = default;
				return ref value;
			}
		}
	}
}