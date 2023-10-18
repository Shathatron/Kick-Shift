namespace Moball
{
    using System;
using UnityEngine;

[System.Flags]
public enum AxisFlag
{
	None = 0,
	X = 1 << 0,
	Y = 1 << 1,
	Z = 1 << 2,
	XY = X | Y,
	All = X | Y | Z,
}

public static class MathUtils
{
	public static Vector3 WithScaledX(this Vector3 v3, float x)
	{
		return new Vector3(v3.x * x, v3.y, v3.z);
	}

	public static Vector3 WithScaledY(this Vector3 v3, float y)
	{
		return new Vector3(v3.x, v3.y * y, v3.z);
	}

	public static Vector3 WithScaledZ(this Vector3 v3, float z)
	{
		return new Vector3(v3.x, v3.y, v3.z * z);
	}	

	public static Vector3 WithX(this Vector3 v3, float x)
	{
		return new Vector3(x, v3.y, v3.z);
	}

	public static Vector3 WithY(this Vector3 v3, float y)
	{
		return new Vector3(v3.x, y, v3.z);
	}
	
	public static Vector2 WithX(this Vector2 v3, float x)
	{
		return new Vector2(x, v3.y);
	}

	public static Vector2 WithY(this Vector2 v3, float y)
	{
		return new Vector2(v3.x, y);
	}

	public static Vector3 WithZ(this Vector3 v3, float z)
	{
		return new Vector3(v3.x, v3.y, z);
	}

	public static Vector2 ToVector2_XY(this Vector3 v3)
	{
		return new Vector2(v3.x, v3.y);
	}

	public static Vector2 ToVector2_XZ(this Vector3 v3)
	{
		return new Vector2(v3.x, v3.z);
	}

	public static Vector3 AngleToVector3_X0Z(this float angle)
	{
		return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0.0f, Mathf.Cos(angle * Mathf.Deg2Rad));
	}

	public static Vector2 AngleToVector2(this float angle)
	{
		return RadianToVector2(angle * Mathf.Deg2Rad);
	}

	public static Vector2 Perpendicular(this Vector2 v)
	{
		return new Vector2(-v.y, v.x);
	}

	public static Vector2 RadianToVector2(this float angle)
	{
		return new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
	}

	public static ushort RadianToUShort(this float radian)
	{
		return (ushort)(((radian / (Mathf.PI * 2.0f)) * 65536.0f));
	}

	public static float UShortToRadian(ushort unsignedShort)
	{
		return ((float)unsignedShort / 65536.0f) * Mathf.PI * 2.0f;
	}

	public static int Sin2048FromUShort(ushort unsignedShort)
	{
		return Mathf.RoundToInt(Mathf.Sin(UShortToRadian(unsignedShort)) * 2048.0f);
	}

	public static float Cos2048FromUShort(ushort unsignedShort)
	{
		return Mathf.RoundToInt(Mathf.Cos(UShortToRadian(unsignedShort)) * 2048.0f);
	}

	public static float Squared(this float val)
	{
		return val * val;
	}

	public static float Cubed(this float val)
	{
		return val * val * val;
	}

	public static int Squared(this int val)
	{
		return val * val;
	}

	public static int RoundToInt(this float val)
	{
		return Mathf.RoundToInt(val);
	}

	public static int RoundToInt(this double val)
	{
		return (int)System.Math.Round(val);
	}

	public static int FloorToInt(this float val)
	{
		return Mathf.FloorToInt(val);
	}

	public static int CeilToInt(this float val)
	{
		return Mathf.CeilToInt(val);
	}

	public static int FloorToInt(this double val)
	{
		return (int)System.Math.Floor(val);
	}

	public static float Round(this float val)
	{
		return Mathf.Round(val);
	}

	public static float Floor(this float val)
	{
		return Mathf.Floor(val);
	}

	public static float Ceil(this float val)
	{
		return Mathf.Ceil(val);
	}

	public static float Sign(this float val)
	{
		return Mathf.Sign(val);
	}

	public static int SignInt(this float val)
	{
		return (int)Mathf.Sign(val);
	}
	
	public static int SignInt(this int val)
	{
		return (int)Mathf.Sign(val);
	}

	public static Vector3 GetPointOnPlane(this Ray ray, Plane plane)
	{
		float distance = 0.0f;

		plane.Raycast(ray, out distance);
		return ray.GetPoint(distance);
	}

	public static Plane GetPlane(this Transform trans)
	{
		var plane = new Plane();
		plane.SetNormalAndPosition(trans.forward, trans.position);
		
		return plane;
	}

	public static Vector4 AsVector4(this Plane plane)
	{
		var norm = plane.normal;
		
		return new Vector4(norm.x, norm.y, norm.z, plane.distance);
	}
	
	public static int SmallestDifference(ushort a, ushort b)
	{
		short result = (short)(a - b);
		return (int)result;
	}

	public static int Repeat(this int val, int m)
	{	
		return val - RoundedFloor(val, m);
	}

	public static int RoundedFloor(this int val, int length)
	{
		var result = (val / length) * length;
		if(val < 0 && result != val)
		{
			result -= length;
		}

		return result;
	}

	public static int RoundedCeil(this int val, int length)
	{
		var result = Mathf.CeilToInt((float)val / (float)length) * length;
		if(result < val)
		{
			result += length;
		}

		return result;
	}

	public static int RoundedNearest(this int val, int length)
	{
		var result = Mathf.RoundToInt(((float)val / (float)length)) * length;

		return result;
	}

	public static float RoundedNearest(this float val, float length)
	{
		var result = (val / length).Round() * length;

		return result;
	}

	public static float RoundedFloor(this float val, float length)
	{
		var result = (val / length).Floor() * length;

		return result;
	}

	public static float RoundedCeil(this float val, float length)
	{
		var result = (val / length).Ceil() * length;

		return result;
	}

	public static uint Repeat(this uint val, uint m)
	{	
		return val - RoundedFloor(val, m);
	}

	public static uint RoundedFloor(this uint val, uint length)
	{
		var result = (val / length) * length;
		return result;
	}

	public static Vector2 NextPointInCircle(this System.Random rand)
	{
		var a = (float)rand.NextDouble();
		var b = (float)rand.NextDouble();

		if(b < a)
		{
			Utils.Swap(ref a, ref b);
		}
		
		return new Vector2(b*Mathf.Cos(2.0f*Mathf.PI*a/b), b*Mathf.Sin(2.0f*Mathf.PI*a/b));
	}
	
	public static int DeltaRepeat(int a, int b, int repeat)
	{
		var num = (a - b).Repeat(repeat);
		if (num > repeat / 2)
		{
			num -= repeat;
		}
		return num;
	}

	public static float ScalarInverse(this float val)
	{
		return 1.0f / val;
	}

	public static Vector2 ScalarInverse2(this Vector2 val)
	{
		return new Vector2(1.0f / val.x, 1.0f / val.y);
	}

	public static Vector3 ScalarInverse3(this Vector3 val)
	{
		return new Vector3(1.0f / val.x, 1.0f / val.y, 1.0f / val.z);
	}

	public static Vector2 Scale2(this Vector2 val, Vector2 by)
	{
		return Vector2.Scale(val, by);
	}

	public static Vector3 Scale3(this Vector3 val, Vector3 by)
	{
		return Vector3.Scale(val, by);
	}
	
	public static void SwapIfOutOfOrder(ref float smaller, ref float larger)
	{
		if(smaller > larger)
		{
			var temp = smaller;
			smaller = larger;
			larger = temp;
		}		
	}
	
	public static void SwapIfOutOfOrder(ref int smaller, ref int larger)
	{
		if(smaller > larger)
		{
			var temp = smaller;
			smaller = larger;
			larger = temp;
		}		
	}

	public static Vector2 RotatedVector(this Vector2 v, float angle)
	{
		var cs = Mathf.Cos(angle * Mathf.Deg2Rad);
		var sn = Mathf.Sin(angle * Mathf.Deg2Rad);
		return new Vector2(v.x * cs - v.y * sn, v.x * sn + v.y * cs);
	}

	public static int ToInt(this bool val)
	{
		return val ? 1 : 0;
	}

	public static float ToFloat(this bool val)
	{
		return val ? 1.0f : 0.0f;
	}

	public static int ToSign(this bool val)
	{
		return val ? 1 : -1;
	}

	public static bool IsFinite(this float val)
	{
		return !float.IsInfinity(val) && !float.IsNaN(val);
	}

	public static bool IsFinite(this double val)
	{
		return !double.IsInfinity(val) && !double.IsNaN(val);
	}

	public static float Repeat(this float val, float repeat)
	{
		return Mathf.Repeat(val, repeat);
	}

	public static Vector3 Repeat(this Vector3 val, float repeat)
	{
		return new Vector3(Mathf.Repeat(val.x,repeat), Mathf.Repeat(val.y,repeat), Mathf.Repeat(val.z,repeat));
	}

	public static Vector3 RoundedNearest(this Vector3 val, float length)
	{
		return new Vector3(val.x.RoundedNearest(length), val.y.RoundedNearest(length), val.z.RoundedNearest(length));
	}
	
	public static Quaternion RoundedNearest(this Quaternion val, float length)
	{
		return new Quaternion(val.x.RoundedNearest(length), val.y.RoundedNearest(length), val.z.RoundedNearest(length), val.w.RoundedNearest(length));
	}

	public static Vector2 Repeat(this Vector2 val, float repeat)
	{
		return new Vector2(Mathf.Repeat(val.x,repeat), Mathf.Repeat(val.y,repeat));
	}

	public static Vector3 ToVector3_X0Z(this Vector2 v2)
	{
		return new Vector3(v2.x, 0.0f, v2.y);
	}

	public static Vector3 ToVector3_XY0(this Vector2 v2)
	{
		return new Vector3(v2.x, v2.y, 0.0f);
	}

	public static Vector4 ToTexture_ST(this Rect rect)
	{
		return new Vector4(rect.x, rect.y, rect.width, rect.height);
	}

	public static Vector2 Normalize(this Rect rect, Vector2 p)
	{
		return new Vector2(
			p.x.LinearMap(rect.xMin, rect.xMax),
			p.y.LinearMap(rect.yMin, rect.yMax)
			);
	}

	public static bool FitsWithin(this Vector2Int val, Vector2Int size)
	{
		return val.x <= size.x && val.y <= size.y;
	}
	
	
	public static bool FitsWithin(this RectInt val, RectInt bounds)
	{
		return bounds.Contains(val.min) && bounds.Contains(val.max - Vector2Int.one);
	}
	
	public static int MaxValue(this Vector2Int val)
	{
		return val.x.Max(val.y);
	}
	
	public static int MinValue(this Vector2Int val)
	{
		return val.x.Min(val.y);
	}

	public static float GetAngleFromXZ(this Vector3 v3)
	{
		if(v3.sqrMagnitude > float.Epsilon)
		{
			return Mathf.Atan2(v3.x, v3.z) * Mathf.Rad2Deg;
		}

		return 0.0f;
	}


	public static float Atan2(this Vector2 v2)
	{
		return Mathf.Atan2(v2.x, v2.y);
	}

	public static float ToAngle(this Vector2 v2)
	{
		return Mathf.Atan2(v2.x, v2.y) * Mathf.Rad2Deg;
	}

	public static float DeltaRadian(float a, float b)
	{
		return DeltaRepeat(a, b, Mathf.PI * 2.0f);
	}

	public static float RadianRepeat(this float val)
	{
		return Mathf.Repeat(val, Mathf.PI * 2.0f);
	}

	public static float MaxAbs(this float val, float other)
	{
		return other.Abs() > val.Abs() ? other : val;
	}
	
	public static double MaxAbs(this double val, double other)
	{
		return other.Abs() > val.Abs() ? other : val;
	}

	public static float Abs(this float val)
	{
		return Mathf.Abs(val);
	}
	
	public static double Abs(this double val)
	{
		return System.Math.Abs(val);
	}

	public static int Abs(this int val)
	{
		return Mathf.Abs(val);
	}
	
	public static Vector2 Abs(this Vector2 val)
	{
		return new Vector2(val.x.Abs(), val.y.Abs());
	}
	
	public static Vector3 Abs(this Vector3 val)
	{
		return new Vector3(val.x.Abs(), val.y.Abs(), val.z.Abs());
	}
	
	public static Vector4 Abs(this Vector4 val)
	{
		return new Vector4(val.x.Abs(), val.y.Abs(), val.z.Abs(), val.w.Abs());
	}

	public static int Clamp(this int val, int min, int max)
	{
		return Mathf.Clamp(val, min, max);
	}

	public static int Max(this int val, int max)
	{
		return Mathf.Max(val, max);
	}

	public static int Min(this int val, int min)
	{
		return Mathf.Min(val, min);
	}

	public static int ValueAboveZeroOfDefault(this int val, int defaultValue)
	{
		return val > 0 ? val : defaultValue;
	}
	
	public static float LargestElement(this Vector2 val)
	{
		return val.x.Max(val.y);
	}
	
	public static float LargestElement(this Vector3 val)
	{
		return val.x.Max(val.y).Max(val.z);
	}

	public static float ClampMagnitude(this float val, float maxMagnitude)
	{
		return Mathf.Clamp(val, -maxMagnitude, maxMagnitude);
	}
	
	public static bool ClampDistance(ref float val, float clampWithin, float maxDistance)
	{
		var min = clampWithin - maxDistance.Max(0.0f);
		var max = clampWithin + maxDistance.Max(0.0f);

		if (val < min)
		{
			val = min;
			return true;
		}

		if (val > max)
		{
			val = max;
			return true;
		}

		return false;
	}

	public static float ClampDistance(this float val, float clampWithin, float maxDistance)
	{
		var result = val;
		ClampDistance(ref result, clampWithin, maxDistance);
		return result;
	}

	public static float Clamp01(this float val)
	{
		return Mathf.Clamp01(val);
	}

	public static float Clamp(this float val, float min, float max)
	{
		return Mathf.Clamp(val, min, max);
	}
	
	public static float Clamp(this float val, FloatRange range)
	{
		return Mathf.Clamp(val, range.min, range.max);
	}

	public static float Max(this float val, float max)
	{
		return Mathf.Max(val, max);
	}

	public static float Min(this float val, float min)
	{
		return Mathf.Min(val, min);
	}

	public static Vector2 ClampMagnitude(this Vector2 val, float maxMagnitude)
	{
		var mag = val.magnitude;

		if(mag > maxMagnitude)
		{
			return (maxMagnitude / mag) * val;
		}

		return val;
	}
	
	public static bool IsAbout(this int val, int other)
	{
		return val == other;
	}

	public static float GetChangeAmountFromFriction(float friction, float deltaTime)
	{
		return Mathf.Pow(friction, deltaTime * 60.0f);
	}

	public static double LerpUntilCloseWithFrictionUnscaledTime(this double current, double desired, double friction)
	{
		return LerpUntilClose(current, desired, System.Math.Pow(friction, Time.unscaledDeltaTime * 60.0));
	}
	
	public static float LerpUntilCloseWithFrictionUnscaledTime(this float current, float desired, float friction)
	{
		return LerpUntilClose(current, desired, Mathf.Pow(friction, Time.unscaledDeltaTime * 60.0f));
	}

	public static bool UpdateUntilClose(this ref double current, double desired, double changeAmount,
		double epsilon = 0.001)
	{
		return current.UpdateValue(current.LerpUntilClose(desired, changeAmount, epsilon));
	}

	public static double LerpUntilClose(this double current, double desired, double changeAmount, double epsilon = 0.001)
	{
		var result = Lerp(desired, current, changeAmount);
		if (result.IsAbout(desired, epsilon))
		{
			result = desired;
		}
		return result;
	}

	public static float LerpUntilClose(this float current, float desired, float changeAmount, float epsilon = 0.001f)
	{
		var result = Lerp(desired, current, changeAmount);
		if (result.IsAbout(desired, epsilon))
		{
			result = desired;
		}
		return result;
	}
	
	public static double LerpUnclamped(double a, double b, double t)
	{
		return a * (1.0 - t) + b * t;
	}
	
	public static int Difference(uint a, uint b)
	{
		return (int)(a - b);
	}
	
	public static float InverseLerp(uint start, uint end, uint time)
	{
		return Mathf.InverseLerp(Difference(start, time), Difference(end, time), 0.0f);
	}

	public static float InverseLerpUnclamped(float start, float end, float time)
	{
		return (time - start) / (end - start);
	}

	public static float LinearMapTo(this float value, float toMin, float toMax)
	{
		return Mathf.Lerp(toMin, toMax, value); 
	}

	public static float LinearMapWithLength(this float value, float fromMin, float length, float toMin = 0.0f, float toMax = 1.0f)
	{
		return value.LinearMap(fromMin, fromMin + length, toMin, toMax);
	}

	public static float LinearMap(this float value, float fromMin, float fromMax, float toMin = 0.0f, float toMax = 1.0f)
	{
		return Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));
	}

	public static float LinearMap(this float value, FloatRange range, FloatRange toRange)
	{
		return value.LinearMap(range.min, range.max, toRange.min, toRange.max);
	}
	
	public static float MoveTowards(this float val, float target, float maxDelta, float positiveScalar = 1.0f)
	{
		return Mathf.MoveTowards(val, target, maxDelta * (target > val ? positiveScalar : 1.0f));
	}

	public static float MoveUnilateralTowards(this float val, bool isOne, float maxUpDelta, float maxDownDelta = -1.0f)
	{
		if(isOne)
		{
			return Mathf.MoveTowards(val, 1.0f, maxUpDelta);
		}
		else
		{
			if(maxDownDelta < 0.0f)
			{
				maxDownDelta = maxUpDelta;
			}
			return Mathf.MoveTowards(val, 0.0f, maxDownDelta);
		}
	}

	public static bool IsAbout(this float val, float isAbout, float epsilon = 0.001f)
	{
		var min = val.Min(isAbout);
		var max = val.Max(isAbout);
		return (max - min) <= epsilon;
	}
	
	public static bool IsAbout(this double val, double isAbout, double epsilon = 0.001)
	{
		var min = System.Math.Min(val, isAbout);
		var max = System.Math.Max(val, isAbout);
		return (max - min) <= epsilon;
	}
	
	public static bool IsAbout(this Rect val, Rect isAbout, float epsilon = 0.001f)
	{
		return
			val.xMin.IsAbout(isAbout.xMin) &&
			val.yMin.IsAbout(isAbout.yMin) &&
			val.xMax.IsAbout(isAbout.xMax) &&
			val.yMax.IsAbout(isAbout.yMax);
	}

	public static bool IsAbout(this Vector3 val, Vector3 isAbout, float epsilon = 0.001f)
	{
		return 
			val.x.IsAbout(isAbout.x, epsilon) && 
			val.y.IsAbout(isAbout.y, epsilon) &&
			val.z.IsAbout(isAbout.z, epsilon);
	}
	
	public static bool IsAbout(this Vector4 val, Vector4 isAbout, float epsilon = 0.001f)
	{
		return 
			val.x.IsAbout(isAbout.x, epsilon) && 
			val.y.IsAbout(isAbout.y, epsilon) &&
			val.z.IsAbout(isAbout.z, epsilon) &&
			val.w.IsAbout(isAbout.w, epsilon);
	}

	public static bool IsAbout(this Quaternion val, Quaternion isAbout, float epsilon = 0.001f)
	{
		return 
			val.x.IsAbout(isAbout.x, epsilon) && 
			val.y.IsAbout(isAbout.y, epsilon) &&
			val.z.IsAbout(isAbout.z, epsilon) &&
			val.w.IsAbout(isAbout.w, epsilon);
	}


	public static bool IsAbout(this Vector2 val, Vector2 isAbout, float epsilon = 0.001f)
	{
		return 
			val.x.IsAbout(isAbout.x, epsilon) && 
			val.y.IsAbout(isAbout.y, epsilon);
	}


	public static bool IsAbout(this Matrix4x4 val, Matrix4x4 isAbout, float epsilon = 0.001f)
	{
		for(var m = 0; m < 16; ++m)
		{
			if(val[m].IsAbout(isAbout[m], epsilon) == false)
			{
				return false;
			}
		}

		return true;
	}

	public static bool IsAbout(this Color val, Color isAbout, float epsilon = 0.001f)
	{
		return
			val.r.IsAbout(isAbout.r, epsilon) &&
			val.g.IsAbout(isAbout.g, epsilon) &&
			val.b.IsAbout(isAbout.b, epsilon) &&
			val.a.IsAbout(isAbout.a, epsilon);
	}
	
	public static Vector3 Sign(this Vector3 val)
	{
		return new Vector3(val.x.Sign(), val.y.Sign(), val.z.Sign());
	}

	public static float Lerp(float a, float b, float t)
	{
		return Mathf.Lerp(a, b, t);
	}
	public static float LerpWrapped(float a, float b, float t, float wrappingCeiling)
	{
		var halfCeiling = (double) wrappingCeiling / 2.0;
		var num = Mathf.Repeat(b - a, wrappingCeiling);
		if ((double) num > halfCeiling)
			num -= wrappingCeiling;
		return a + num * Mathf.Clamp01(t);
	}
	
	public static double Lerp(double a, double b, double t)
	{
		t = System.Math.Max(t, 0);
		t = System.Math.Min(t, 1);
		
		return a * (1.0 - t) + b * t;
	}

	public static Vector3 ClampMagnitude(this Vector3 val, float maxMagnitude)
	{
		var mag = val.magnitude;

		if(mag > maxMagnitude)
		{
			return (maxMagnitude / mag) * val;
		}

		return val;
	}

	public static float SmallestAxis(this Vector3 vec)
	{
		var result = vec.x;

		if (vec.y.Abs() < result.Abs())
		{
			result = vec.y;
		}
		
		if (vec.z.Abs() < result.Abs())
		{
			result = vec.z;
		}

		return result;
	}

	public static float DeltaRepeat(float a, float b, float repeat)
	{
		float num = Mathf.Repeat(a - b, repeat);
		if (num > repeat * 0.5f)
		{
			num -= repeat;
		}
		return num;
	}

	public static Vector3 DeltaRepeat(Vector3 a, Vector3 b, float repeat)
	{
		return new Vector3(	DeltaRepeat(a.x, b.x, repeat), 
			DeltaRepeat(a.y, b.y, repeat),
			DeltaRepeat(a.z, b.z, repeat));
	}

	public static Vector2 DeltaRepeat(Vector2 a, Vector2 b, float repeat)
	{
		return new Vector2(	DeltaRepeat(a.x, b.x, repeat), 
			DeltaRepeat(a.y, b.y, repeat));
	}

	public static int Area(this RectInt rect)
	{
		return rect.size.x * rect.size.y;
	}
	
	public static float Area(this Rect rect)
	{
		return rect.size.x * rect.size.y;
	}
	
	
	public static bool IsValidQuaternion(this Quaternion value)
	{
		return value.x.IsFinite() && value.y.IsFinite() && value.z.IsFinite() && value.w.IsFinite() && value.IsAbout(default) == false;
	}

	public static Quaternion ValidOrIdentity(this Quaternion value) =>
		value.IsValidQuaternion() ? value : Quaternion.identity;
	
	public static bool IsValidVector(this Vector3 value)
	{
		return value.x.IsFinite() && value.y.IsFinite() && value.z.IsFinite();
	}

	public static Quaternion RotationBetween(Quaternion fromRotation, Quaternion toRotation)
	{
		return Quaternion.Inverse(fromRotation) * toRotation;
	}

	public static Vector3 TorqueVectorFromToRotation(Quaternion fromRotation, Quaternion toRotation)
	{
		var newRotationalOffset = Quaternion.Inverse(fromRotation) * toRotation;
		
		if(IsValidQuaternion(newRotationalOffset))
		{
			newRotationalOffset.ToAngleAxis(out var angle, out var axis);
			
			if(angle > 180.0f)
			{
				angle = Mathf.Abs(angle - 360.0f);
				axis *= -1.0f;
			}
			
			if(IsValidVector(axis) && axis.sqrMagnitude > 0.0f)
			{
				axis.Normalize();
				return axis * angle;
			}
		}

		return Vector3.zero;
	}

	public static Quaternion QuaternionFromMatrix(this Matrix4x4 m)
	{
		// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
		Quaternion q = new Quaternion();
		q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2; 
		q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2; 
		q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2; 
		q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2; 
		q.x *= Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
		q.y *= Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
		q.z *= Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
		return q;
	}
	
	public static Vector3 ForwardVector(this Quaternion rotation)
	{
		float num = rotation.x * 2f;
		float num2 = rotation.y * 2f;
		float num3 = rotation.z * 2f;
		float num4 = rotation.x * num;
		float num5 = rotation.y * num2;
		//float num6 = rotation.z * num3;
		//float num7 = rotation.x * num2;
		float num8 = rotation.x * num3;
		float num9 = rotation.y * num3;
		float num10 = rotation.w * num;
		float num11 = rotation.w * num2;
		//float num12 = rotation.w * num3;
		Vector3 result;
		result.x = (num8 + num11);
		result.y = (num9 - num10);
		result.z = (1f - (num4 + num5));
		return result;
	}

	public static bool Between(this float val, float min, float max)
	{
		return val >= min && val <= max;
	}

	public static Matrix4x4 ScaleAroundPoint(Matrix4x4 mtx, Vector3 point, float scale)
	{
		return Matrix4x4.Translate(point) * (Matrix4x4.Scale(Vector3.one * scale) * (Matrix4x4.Translate(-point) * mtx));
	}

	public static Vector3 GetTranslation(this Matrix4x4 m)
	{
		return new Vector3(m[0,3], m[1,3], m[2,3]);
	}

	public static Quaternion GetRotation(this Matrix4x4 m)
	{
		Quaternion q = Quaternion.identity;

		Vector3 up	 		= m.MultiplyVector(Vector3.up);
		Vector3 forward 	= m.MultiplyVector(Vector3.forward);

		if(up.sqrMagnitude > 0.0f && forward.sqrMagnitude > 0.0f)
		{
			q = Quaternion.LookRotation(forward, up);
		}

		return q;
	}

	public static Vector3 GetScale(this Matrix4x4 matrix)
	{
		Vector3 scale = new Vector3(
			matrix.GetColumn(0).magnitude,
			matrix.GetColumn(1).magnitude,
			matrix.GetColumn(2).magnitude
		);

		if (matrix.determinant < 0)
		{
			scale.x *= -1;
		}
		return scale;
	}
	
	public const float e = 2.7182818284590452353602874713527f;
	public const float eInv = 0.3678794411714423215955237701614f;

	public static float GetPositionOffsetForTime(float t, float friction, float acceleration, float currentVelocity)
	{
		var tmp = 0.0f;
		return GetPositionOffsetForTime(t, friction, acceleration, currentVelocity, out tmp);
	}

	public static float GetPositionOffsetForTime(float t, float friction, float acceleration, float currentVelocity, out float newVelocity)
	{
		var frictionLog = -Mathf.Log(friction);
		var T = 1.0f / frictionLog;
		var E = Mathf.Pow(e, -t * frictionLog);
		var Vt = acceleration * T; // terminal velocity

		var d = Vt * t + currentVelocity * T * (1.0f - E) + Vt * T * (E - 1.0f);

		newVelocity = currentVelocity * E + Vt * (1.0f - E);

		return d;
	}

	public static float GetTerminalVelocity(float friction, float acceleration)
	{
		var frictionLog = -Mathf.Log(friction);
		var T = 1.0f / frictionLog;
		return acceleration * T; // terminal velocity
	}
    
	public static Vector3 MapOntoSphere(this Vector3 pos, Vector3 sphereCenter, float radius)
	{
		var positionOnSphere = (pos - sphereCenter).normalized * radius + sphereCenter;

		return positionOnSphere;
	}
	
	public static float FindBlendFromWeights(ReadOnlySpan<float> weights, out int fromIndex, out int toIndex)
	{
		toIndex = 0;
		fromIndex = 0;
		for (int i = 1; i < weights.Length; i++)
		{
			if (weights[i] > weights[toIndex])
			{
				fromIndex = toIndex;
				toIndex = i;
			}
			else if (weights[i] > weights[fromIndex] || fromIndex == toIndex)
			{
				fromIndex = i;
			}
		}

		var fromWeight = weights[fromIndex].Max(0.0f);
		var toWeight = weights[toIndex].Max(0.0f);

		var totalWeight = fromWeight + toWeight;
		
		if(totalWeight > 0.0f)
		{
			return toWeight / totalWeight;
		}

		return 0.0f;
	}
	
	public static Vector2 RelativePosition(this Rect rect, Vector2 screenPosition)
	{
		return new Vector2(Mathf.Clamp01((screenPosition.x - rect.x) / rect.width), Mathf.Clamp01((screenPosition.y - rect.y) / rect.height));
	}
	
	public static Vector2 RelativePositionUnclamped(this Rect rect, Vector2 screenPosition)
	{
		return new Vector2(InverseLerpUnclamped(rect.xMin, rect.xMax, screenPosition.x), InverseLerpUnclamped(rect.yMin, rect.yMax, screenPosition.y));
	}
	
	public static Vector4 InvertByAmount(this Vector4 weights, Vector4 invertAmount)
	{
		return Vector4.Scale(weights, Vector4.one - invertAmount) + Vector4.Scale(Vector4.one - weights, invertAmount);
	}
	
	public static float InvertByAmount(this float value, float invertAmount)
	{
		return value * (1.0f - invertAmount) + (1.0f - value) * (invertAmount);
	}

	public static float OneMinus(this float value)
	{
		return 1.0f - value;
	}

	public static int NumDigits(this int val)
	{
		val = val.Abs();

		var result = 0;

		while (val != 0)
		{
			result++;
			val /= 10;
		}

		return result;
	}

}
}