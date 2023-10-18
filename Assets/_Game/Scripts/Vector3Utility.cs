using UnityEngine;

namespace Moball {
    public static class Vector3Utility {
    	public static float Max(Vector3 v) => Mathf.Max(v.x, v.y, v.z);
    }
}