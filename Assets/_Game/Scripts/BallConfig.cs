#nullable enable

using UnityEngine;
using NaughtyAttributes;

namespace Moball {
    [CreateAssetMenu(menuName = "Moball/BallConfig")]
    public class BallConfig : ScriptableObject {
        [SerializeField] float _scale = 1f;
        [SerializeField] float _airSpinScale = 1f;
        [SerializeField] float _groundSpinScale = 1f;
        [SerializeField] float _gravity = 9.8f;
        [InfoBox("Mass is currently ignored in most interactions")]
        [SerializeField] float _mass = 0.05f;

        public float Scale => _scale;
        public float AirSpinScale => _airSpinScale;
        public float GroundSpinScale => _groundSpinScale;
        public float Gravity => _gravity;
        public float Mass => _mass;
    }
}
