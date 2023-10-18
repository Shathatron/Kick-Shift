using UnityEngine;
using System.Collections.Generic;

namespace Moball {
    public static class GameObjectUtility {
      static readonly List<Transform> _childrenTemp = new List<Transform>();
      public static void SetLayerRecursively(
        this GameObject gameObject,
        int layer
      ) {
        _childrenTemp.Clear();
        gameObject.GetComponentsInChildren(includeInactive: true, _childrenTemp);
        foreach (var child in _childrenTemp) {
            child.gameObject.layer = layer;
        }
      }
    }
}