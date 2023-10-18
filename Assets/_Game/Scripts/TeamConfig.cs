using UnityEngine;
using UnityEngine.Serialization;

namespace Moball {
    [CreateAssetMenu(menuName = "Moball/TeamConfig")]
    public class TeamConfig : ScriptableObject {
        [SerializeField] string _displayName = default;
        [SerializeField] Color _uiColor = Color.white;
        [SerializeField] Color _carColor = Color.white;
        [SerializeField] Color _arenaColor = Color.white;
        [SerializeField] Color[] _accentColors = new [] { Color.white };

        public string DisplayName => _displayName;
        public Color UiColor => _uiColor;
        public Color CarColor => _carColor;
        public Color ArenaColor => _arenaColor;
        public bool TryGetAccentColor(int index, out Color color) {
            if (_accentColors.Length == 0) {
                color = Color.white;
                return false;
            }
            var safeIndex = MathUtility.Repeat(index, _accentColors.Length);
            color = _accentColors[safeIndex];
            return true;
        }
    }
}