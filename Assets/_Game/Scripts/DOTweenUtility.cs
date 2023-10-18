using DG.Tweening;
using UnityEngine;

namespace Moball {
  public static class DOTweenUtility {
    public static void Kill(ref Tween tween, bool complete = false) {
      if (tween != null) {
        tween.Kill();
        tween = null;
      }
    }
  }
}
