#nullable enable

using UnityEngine;
using NaughtyAttributes;

namespace Moball {
    enum StaminaStatus {
        Normal,
        Low,
        Warning,
        GassedOut
    }

    class StaminaIndicator : MonoBehaviour {
        [Header("Config")]
        [SerializeField, Range(0, 360)] float _maxFillAngle = 220f;
        [SerializeField] Color _lowColor = Color.red;
        [SerializeField] Color _gassedOutColor = Color.red;
        [SerializeField, CurveRange(0, 0, 1, 1)] AnimationCurve _fillCurve = AnimationCurveUtility.Linear01();

        [Header("References")]
        [SerializeField, Required] SpriteRenderer _outlineRenderer = default!;
        [SerializeField, Required] SpriteRenderer _fillRenderer = default!;
        [SerializeField, Required] GameObject _warningIcon = default!;
        [SerializeField, Required] GameObject _emptyIcon = default!;

        Color _defaultOutlineColor;
        Color _defaultFillColor;

        static class ShaderProperty {
            public static readonly int _Fill = Shader.PropertyToID(nameof(_Fill));
            public static readonly int _Color = Shader.PropertyToID(nameof(_Color));
        }

        void Awake() {
            _defaultOutlineColor = _outlineRenderer.color;
            _defaultFillColor = _fillRenderer.material.GetColor(ShaderProperty._Color);
        }

        public void Initialize(int layer) {
            gameObject.SetLayerRecursively(layer);
        }

        public void SetPlayerPosition(Vector3 position) {
            position.y = 0;
            transform.position = position;
        }

        public void SetCameraFacing(Vector3 direction) {
            var projectedDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
            transform.LookAt(transform.position + projectedDirection, Vector3.up);
        }

        public void Set(
            float normalizedFillAmount,
            StaminaStatus status
        ) {
            _fillRenderer.material.SetFloat(
                ShaderProperty._Fill,
                Mathf.Max(
                    0,
                    _fillCurve.Evaluate(normalizedFillAmount) * _maxFillAngle
                )
            );
            _fillRenderer.material.SetColor(
                ShaderProperty._Color,
                status switch {
                    StaminaStatus.Low => _lowColor,
                    StaminaStatus.GassedOut => _gassedOutColor,
                    _ => _defaultFillColor,
                }
            );
            _outlineRenderer.color = status switch {
                StaminaStatus.GassedOut => _gassedOutColor,
                _ => _defaultOutlineColor
            };
            _warningIcon.SetActive(status == StaminaStatus.Warning);
            _emptyIcon.SetActive(status == StaminaStatus.GassedOut);
        }
    }
}
