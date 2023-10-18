#nullable enable

using UnityEngine.InputSystem;
using UnityEngine;
using NaughtyAttributes;

namespace Moball {
    public enum CarRotationAxis {
        Roll,
        Yaw
    }

    [CreateAssetMenu(menuName = "Moball/CarConfig")]
    public class CarConfig : ScriptableObject {
        [Header("Camera")]
        [SerializeField]
        PlayerCameraMode[] _cameraModeOrder = new PlayerCameraMode[] {
            PlayerCameraMode.FixedAngleCar,
            PlayerCameraMode.FixedAngleCarAndBall,
            PlayerCameraMode.OrbitCar,
            PlayerCameraMode.OrbitCarTargetBall,
        };
        [SerializeField] int _initialCameraModeIndex = 0;
        [SerializeField] bool _invertCameraX = false;
        [SerializeField] bool _invertCameraY = false;

        [Header("Controls")]
        [InfoBox("Temporarily ignoring input actions field", EInfoBoxType.Warning)]
        [SerializeField, Required] InputActionAsset _inputActions = null!;
        [SerializeField] bool _allowBoostWhilePivoting = false;
        [SerializeField] bool _doubleTapToBoost = true;
        [SerializeField] float _doubleTapToBoostMaxDelay = 0.6f;
        [SerializeField] bool _automaticRecoveryRoll = false;
        [SerializeField] bool _automaticAcceleration = true;
        [SerializeField] CarRotationAxis _defaultAirRotationAxis = CarRotationAxis.Yaw;
        [SerializeField] bool _diveOnRelease = true;

        [Header("Pivot")]
        [SerializeField] float _pivotKickBallSpinScale = 1f;
        [Tooltip("Speed given to ball when pivoting for a hit (Hz * m/s)")]
        [SerializeField] float _pivotKickBallSpeed = 1f;

        [Header("Pulse")]
        [SerializeField] float _minPulseRadius = 3f;
        [SerializeField] float _maxPulseRadius = 5f;
        [SerializeField, CurveRange(0, 0, 1, 1)] AnimationCurve _pulseDistanceCurve = AnimationCurveUtility.ReverseLinear01();
        [SerializeField] float _maxPulseCharge = 10f;
        [SerializeField] float _maxPulseBallSpeed = 20f;
        [SerializeField, CurveRange(0, 0, 1, 1)] AnimationCurve _pulseChargeCurve = AnimationCurveUtility.Linear01();
        [SerializeField] LayerMask _pulseLayerMask = Physics.DefaultRaycastLayers;

        [Header("Tilt")]
        [InfoBox("Angle to adjust ball kick height while tilting. Negative is down, positive is up")]
        [SerializeField] float _lowTiltKickAngle = 10f;
        [SerializeField] float _highTiltKickAngle = -5f;

        [Header("Launch")]
        [SerializeField] float _launchChargeDuration = 1f;
        [SerializeField] float _launchMinSpeed = 5f;
        [SerializeField] float _launchMaxSpeed = 10f;
        [SerializeField] float _launchMinStaminaCost = 5f;
        [SerializeField] float _launchMaxStaminaCost = 10f;
        [SerializeField] float _launchBallLockOnDistance = 10f;
        [SerializeField] float _launchDomeMinDuration = 0.5f;
        [SerializeField] float _launchDomeMaxDuration = 1f;

        [field: SerializeField] public float LaunchMaxCount { get; private set; } = 3.0f;
        [field: SerializeField] public float LaunchRechargeSpeed { get; private set; } = 1.0f;

        public InputActionAsset InputActions => _inputActions;
        public PlayerCameraMode GetCameraMode(int modeIndex) {
            return _cameraModeOrder[modeIndex % _cameraModeOrder.Length];
        }
        public int InitialCameraModeIndex => _initialCameraModeIndex;
        public bool InvertCameraX => _invertCameraX;
        public bool InvertCameraY => _invertCameraY;
        public bool AllowBoostWhilePivoting => _allowBoostWhilePivoting;
        public bool DoubleTapToBoost => _doubleTapToBoost;
        public float DoubleTapToBoostMaxDelay => _doubleTapToBoostMaxDelay;
        public bool AutomaticRecoveryRoll => _automaticRecoveryRoll;
        public bool AutomaticAcceleration => _automaticAcceleration;
        public CarRotationAxis DefaultRotationAxis => _defaultAirRotationAxis;
        public CarRotationAxis AlternateRotationAxis => _defaultAirRotationAxis switch {
            CarRotationAxis.Roll => CarRotationAxis.Yaw,
            CarRotationAxis.Yaw => CarRotationAxis.Roll,
            _ => throw DebugUtility.NoMatch(_defaultAirRotationAxis, nameof(_defaultAirRotationAxis))
        };
        public bool DiveOnRelease => _diveOnRelease;
        public float PivotKickBallSpinScale => _pivotKickBallSpinScale;
        public float PivotKickBallSpeed => _pivotKickBallSpeed;
        public float MinPulseRadius => _minPulseRadius;
        public float MaxPulseRadius => _maxPulseRadius;
        public AnimationCurve PulseDistanceCurve => _pulseDistanceCurve;
        public float MaxPulseCharge => _maxPulseCharge;
        public float MaxPulseBallSpeed => _maxPulseBallSpeed;
        public AnimationCurve PulseChargeCurve => _pulseChargeCurve;
        public LayerMask PulseLayerMask => _pulseLayerMask;
        public float LowTiltKickAngle => _lowTiltKickAngle;
        public float HighTiltKickAngle => _highTiltKickAngle;
        public float LaunchChargeDuration => _launchChargeDuration;
        public float LaunchMinSpeed => _launchMinSpeed;
        public float LaunchMaxSpeed => _launchMaxSpeed;
        public float LaunchMinStaminaCost => _launchMinStaminaCost;
        public float LaunchMaxStaminaCost => _launchMaxStaminaCost;
        public float LaunchBallLockDistance => _launchBallLockOnDistance;
        
        public float LaunchDomeMinDuration => _launchDomeMinDuration;
        public float LaunchDomeMaxDuration => _launchDomeMaxDuration;

        [field: SerializeField] public float GroundedFriction { get; set; } = 0.9f;
        
        [field: SerializeField] public float AirFrictionAtTerminalVelocity { get; set; } = 0.9f;
        
        [field: SerializeField] public float AirTerminalVelocity { get; set; } = 50.0f;
    }
}
