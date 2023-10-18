using UnityEngine;

namespace Moball {
    public static class ColorUtility {
        public static Color WithAlpha(this Color color, float a) {
            color.a = a;
            return color;
        }
    }
}