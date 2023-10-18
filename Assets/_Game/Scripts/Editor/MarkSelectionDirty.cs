using UnityEngine;
using UnityEditor;

namespace Moball.Editor {
  public static class MarkSelectionDirty {
    [MenuItem("Moball/Utility/Mark selected assets dirty")]
    public static void Command() {
      foreach (var obj in Selection.objects) {
        EditorUtility.SetDirty(obj);
      }
      Debug.Log($"Marked {Selection.objects.Length} objects dirty");
    }
  }
}