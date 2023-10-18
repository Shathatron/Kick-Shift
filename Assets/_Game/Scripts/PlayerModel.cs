using UnityEngine;
using NaughtyAttributes;
using DG.Tweening;

namespace Moball {
    public class PlayerModel : MonoBehaviour {
        [Header("Effects")]
        [SerializeField] GameObject _boostEffect = default;

        [Header("Body")]
        [SerializeField, Required] Animator _animator = default;
        [SerializeField, Required] Renderer _renderer = default;
        [ValidateInput(nameof(IsBodyMaterialIndexInRange), "Index is out of range")]
        [SerializeField] int _mainMaterialIndex = -1;
        [ValidateInput(nameof(IsBodyMaterialIndexInRange), "Index is out of range")]
        [SerializeField] int _accentMaterialIndex = -1;

        [Header("Wheels")]
        [SerializeField] Renderer[] _frontWheelRenderers = default;
        [ValidateInput(nameof(IsFrontWheelMaterialIndexInRange), "Index is out of range")]
        [SerializeField] int _frontTyreMaterialIndex = -1;
        [SerializeField] Renderer[] _backWheelRenderers = default;
        [ValidateInput(nameof(IsBackWheelMaterialIndexInRange), "Index is out of range")]
        [SerializeField] int _backTyreMaterialIndex = -1;

        [Header("Pulse")]
        [SerializeField] Color _pulseChargeFromColor = Color.yellow;
        [SerializeField] Color _pulseChargeToColor = Color.red;
        [SerializeField] Color _pulseReleaseColor = Color.white;
        [SerializeField] float _pulseDuration = 0.5f;
        [SerializeField] Ease _pulseEase = Ease.Linear;

        Material[] _materials;
        Material[] _backWheelMaterials;
        Material[] _frontWheelMaterials;

        Material _mainMaterial;
        Color _defaultMainColor;
        Tween _pulseTween;

        // -- Public interface --

        static class Property {
            public static readonly int SteerAmount = Animator.StringToHash(nameof(SteerAmount));
            public static readonly int DriveAmount = Animator.StringToHash(nameof(DriveAmount));
            public static readonly int IsPivoting = Animator.StringToHash(nameof(IsPivoting));
            public static readonly int IsCrouching = Animator.StringToHash(nameof(IsCrouching));
            public static readonly int Height = Animator.StringToHash(nameof(Height));
        }

        public bool IsPivoting {
            get => _animator.GetBool(Property.IsPivoting);
            set => _animator.SetBool(Property.IsPivoting, value);
        }

        /// <param name="steerAmount">From -1 (fully left) to 1 (fully right)</param>
        public void SetSteerAmount(float steerAmount) {
            _animator.SetFloat(Property.SteerAmount, steerAmount);
        }

        public bool IsCrouching {
            get => _animator.GetBool(Property.IsCrouching);
            set => _animator.SetBool(Property.IsCrouching, value);
        }

        public bool IsBoostEffectActive {
            get => _boostEffect.activeSelf;
            set => _boostEffect.SetActive(value);
        }

        public void SetPulseCharge(float normalizedCharge) {
            Debug.Assert(
                normalizedCharge >= 0 && normalizedCharge <= 1,
                $"{nameof(normalizedCharge)} must be in range [0,1]"
            );
            DOTweenUtility.Kill(ref _pulseTween);
            _mainMaterial.color = Color.Lerp(
                _pulseChargeFromColor,
                _pulseChargeToColor,
                normalizedCharge
            );
        }

        public void ReleasePulse() {
            DOTweenUtility.Kill(ref _pulseTween);
            _mainMaterial.color = _pulseReleaseColor;
            _pulseTween = _mainMaterial
                .DOColor(_defaultMainColor, _pulseDuration)
                .SetEase(_pulseEase);
        }

        public void SetColors(Color main, Color accent) {
            _materials[_mainMaterialIndex].color = main;
            _defaultMainColor = main;
            _materials[_accentMaterialIndex].color = accent;
            _renderer.materials = _materials;

            _frontWheelMaterials[_frontTyreMaterialIndex].color = accent;
            for (var i = 0; i < _frontWheelRenderers.Length; i++) {
                _frontWheelRenderers[i].materials = _frontWheelMaterials;
            }

            _backWheelMaterials[_backTyreMaterialIndex].color = accent;
            for (var i = 0; i < _backWheelRenderers.Length; i++) {
                _backWheelRenderers[i].materials = _backWheelMaterials;
            }
        }

        // -- Unity events --

        void Awake() {
            IsBoostEffectActive = false;
            InitializeMaterials();
        }

        // -- Private implementation --

        void InitializeMaterials() {
            _materials = _renderer.sharedMaterials;
            _mainMaterial = new Material(_materials[_mainMaterialIndex]);
            _defaultMainColor = _mainMaterial.color;
            _materials[_mainMaterialIndex] = _mainMaterial;
            _materials[_accentMaterialIndex] = new Material(_materials[_accentMaterialIndex]);

            _frontWheelMaterials = _frontWheelRenderers[0].sharedMaterials;
            _frontWheelMaterials[_frontTyreMaterialIndex] = new Material(_frontWheelMaterials[_frontTyreMaterialIndex]);

            _backWheelMaterials = _backWheelRenderers[0].sharedMaterials;
            _backWheelMaterials[_backTyreMaterialIndex] = new Material(_backWheelMaterials[_backTyreMaterialIndex]);
        }

        // -- Validators --

        bool IsBodyMaterialIndexInRange(int index) =>
            IsMaterialIndexInRange(_renderer, index);

        bool IsFrontWheelMaterialIndexInRange(int index) =>
            IsWheelMaterialIndexInRange(_frontWheelRenderers, index);

        bool IsBackWheelMaterialIndexInRange(int index) =>
            IsWheelMaterialIndexInRange(_frontWheelRenderers, index);

        static bool IsWheelMaterialIndexInRange(Renderer[] wheelRenderers, int index) =>
            IsMaterialIndexInRange(
                wheelRenderers == null || wheelRenderers.Length == 0
                    ? null
                    : wheelRenderers[0],
                index
            );

        static bool IsMaterialIndexInRange(Renderer renderer, int index) {
            if (index < 0) return false;
            return renderer
                ? index < renderer.sharedMaterials.Length
                : true;
        }
    }
}
