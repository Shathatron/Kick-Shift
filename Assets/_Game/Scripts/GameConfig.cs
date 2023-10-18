#nullable enable

using UnityEngine;
using NaughtyAttributes;

namespace Moball {
    [CreateAssetMenu(menuName = "Moball/GameConfig")]
    public class GameConfig : ScriptableObject {
        [SerializeField, Required, Expandable] CarConfig _carConfig = null!;
        [SerializeField, Required, Expandable] StaminaConfig _staminaConfig = null!;
        [SerializeField, Required, Expandable] BallConfig _ballConfig = null!;

        public CarConfig CarConfig => _carConfig;
        public StaminaConfig StaminaConfig => _staminaConfig;
        public BallConfig BallConfig => _ballConfig;
    }
}
