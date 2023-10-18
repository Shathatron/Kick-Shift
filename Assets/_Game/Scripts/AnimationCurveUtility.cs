#nullable enable

using UnityEngine;
using System;

namespace Moball {
    public static class AnimationCurveUtility {
        static readonly Keyframe[] _linear01KeyFrames = new [] {
            new Keyframe(0, 0, 1, 1),
            new Keyframe(1, 1, 1, 1)
        };

        static readonly Keyframe[] _reverseLinear01KeyFrames = new [] {
            new Keyframe(1, 0, -1, -1),
            new Keyframe(0, 1, -1, -1)
        };

        public static AnimationCurve Linear01() => new AnimationCurve(_linear01KeyFrames);
        public static AnimationCurve ReverseLinear01() => new AnimationCurve(_reverseLinear01KeyFrames);

        public static float MaxTime(this AnimationCurve curve) => curve.length == 0
            ? 0
            : LastKeyframe(curve).time;

        public static Keyframe LastKeyframe(this AnimationCurve curve) {
            if (curve.length == 0) {
                throw new ArgumentException($"{nameof(AnimationCurve)} has no key frames");
            }
            return curve[curve.length - 1];
        }
    }
}
