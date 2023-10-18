#nullable enable

using UnityEngine;
using NaughtyAttributes;

namespace Moball {
    [CreateAssetMenu(menuName = "Moball/StaminaConfig")]
    public class StaminaConfig : ScriptableObject {
        [Header("Levels")]
        [SerializeField] float _maxStamina = 100f;
        [ValidateInput(nameof(IsStaminaValueInRange), StaminaRangeError)]
        [SerializeField] float _lowStaminaLevel = 50f;
        [ValidateInput(nameof(IsStaminaValueInRange), StaminaRangeError)]
        [SerializeField] float _minVisibleStamina = 20f;
        [ValidateInput(nameof(IsStaminaValueInRange), StaminaRangeError)]
        [SerializeField] float _initialStamina = 40f;

        [Header("Recharge")]
        [SerializeField] float _normalStaminaRechargeRate = 40f;
        [SerializeField] float _lowStaminaRechargeRate = 30f;

        [Header("Costs")]
        [SerializeField] float _boostStaminaRate = 40f;
        [SerializeField] float _pulseStaminaRate = 40f;

        // -- Public interface --

        public float MaxStamina => _maxStamina;
        public float LowStaminaLevel => _lowStaminaLevel;
        public float MinVisibleStamina => _minVisibleStamina;
        public float InitialStamina => _initialStamina;
        public float NormalStaminaRechargeRate => _normalStaminaRechargeRate;
        public float LowStaminaRechargeRate => _lowStaminaRechargeRate;
        public float BoostStaminaRate => _boostStaminaRate;
        public float PulseStaminaRate => _pulseStaminaRate;

        // -- Validators --

        const string StaminaRangeError = "Cannot be greater than max stamina";
        bool IsStaminaValueInRange(float value) =>
            value >= 0 && value <= _maxStamina;
    }
}

