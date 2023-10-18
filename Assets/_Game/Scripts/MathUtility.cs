using UnityEngine;

namespace Moball {
    public static class MathUtility {
        public static float BetterLerpCoefficient(float rate, float deltaTime) {
            return 1f - Mathf.Exp(-rate * deltaTime);
        }

        public static float OldLerpCoefficientToBetterLerpCoefficient(float oldCoeff, float oldFramerate, float deltaTime) {
            oldCoeff = Mathf.Min(oldCoeff, 0.999f);
            var rate = -oldFramerate * Mathf.Log(1 - oldCoeff);
            return BetterLerpCoefficient(rate, deltaTime);
        }

        public static int Repeat(int value, int max) =>
          (value % max + max) % max;

        public static float Remap(this float value, FloatRange from, FloatRange to, bool clamp = true) {
            return Remap(value, from.min, from.max, to.min, to.max, clamp);
        }

        public static float Remap(float value, float min1, float max1, float min2, float max2, bool clamp = true) {
            var range1 = max1 - min1;
            if (range1 != 0) {
                var normal = (value - min1) / range1;
                if (clamp) normal = Mathf.Clamp(normal, 0, 1);
                var range2 = (max2 - min2);
                var result = min2 + normal * range2;

                return result;
            }
            else return 0f;
        }

        public static Vector3 RelativeVelocity(Rigidbody rigidbody) {
            var rb = rigidbody;
            var vl = rb.velocity;
            var rv = rb.transform.InverseTransformDirection(vl).normalized * vl.magnitude;
            return rv;
        }

        public static Vector3 LerpTowards(this Vector3 current, Vector3 target, float changeAmount = 0.1f) {
            var deltaTime = Time.deltaTime * 60;
            var friction = 1f - changeAmount;
            var amount = Mathf.Pow(friction, deltaTime);

            return Vector3.Lerp(target, current, amount);
        }

        public static float LerpTowards(this float current, float target, float changeAmount = 0.1f) {
            var deltaTime = Time.deltaTime * 60;
            var friction = 1f - changeAmount;
            var amount = Mathf.Pow(friction, deltaTime);

            return Mathf.Lerp(target, current, amount);
        }
    }

    [System.Serializable]
    public struct FloatRange {
        public float min;
        public float max;
        public FloatRange(float min, float max) {
            this.min = min;
            this.max = max;
        }

        public static FloatRange FromValue(float value) {
            return new FloatRange(value, value);
        }

        public float Lerp(float value) {
            return Mathf.Lerp(min, max, value);
        }

        public float center {
            get => (min + max) * 0.5f;
        }

        public float length {
            get => (max - min);
        }
    }
}