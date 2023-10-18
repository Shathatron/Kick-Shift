using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Moball {
    public struct Inputs {
        public Vector2 Move;
        public Vector2 Look;
        public float Accelerate;
        public float Brake;
        public bool JumpHold;
        public bool JumpPress;
        public bool Dive;
        public bool FastFall;
        public bool Pivot;
        public bool BoostHold;
        public bool BoostPress;
        public bool RecoveryRoll;
        public bool ChangeAirRotationAxis;
        public bool Pulse;
        public bool Crouch;
        public bool Launch;
    }

    public class Player : MonoBehaviour {
        public const int WheelCount = 4;

        [Header("Config")]
        public CarConfig CarConfig;
        public StaminaConfig StaminaConfig;

        [Header("Controls config")]
        [SerializeField] float _maxAngularVelocity = 30f;
        [SerializeField] float _drivingSpeed = 20f;
        [CurveRange(0, -1, 2, 1)]
        [Tooltip("X axis is current speed divided by target speed. Y axis is acceleration factor, which gets multiplied with 'maxDrivingAcceleration' to find current acceleration")]
        [SerializeField] AnimationCurve _drivingAccelerationCurve;
        [SerializeField] float _maxDrivingAcceleration = 10f;
        [CurveRange(0, 0, 2, 1)]
        [Tooltip("Speed is a multiple of normal driving speed")]
        [SerializeField] AnimationCurve _wheelTurnFactorBySpeed;
        [SerializeField] float _maxWheelTurnDegrees = 45f;
        [CurveRange(0, 0, 1, 1)]
        [Tooltip("Low friction when moving sideways, for sliding")]
        [SerializeField] AnimationCurve _sidewaysnessVsFriction;
        [SerializeField] float _fullSpeedRetainDotProduct = 0.7f;
        [SerializeField] float _regularAngularDrag = 3.5f;
        [SerializeField] float _pivotAngularDrag = 10f;
        [SerializeField] float _pivotFriction = 1f;
        [SerializeField] float _pivotGroundYawAcceleration = 50f;
        [SerializeField] float _pivot180GroundYawAcceleration = 50f;
        [SerializeField] float _endPivotMaxAngularVelocity = 2f;
        [SerializeField] float _pivotForwardRedirectFactor = 1f;
        [SerializeField] float _pivotBackwardsRedirectFactor = 0.7f;
        [SerializeField] float _bigJumpSpeed = 20f;
        [SerializeField] float _smallJumpSpeed = 10f;
        [SerializeField] float _jumpSquatDuration = 5.5f / 60f;
        [SerializeField] float _fastFallMaxSpeed = 30f;
        [SerializeField] float _fastFallAcceleration = 50f;
        [SerializeField] float _diveSpeed = 50f;
        [SerializeField] float _diveOrthogonalVelocityRetention = 0.5f;
        [SerializeField] float _uprightRollTorque = 20f;
        [SerializeField] float _uprightDownForce = 20f;
        [SerializeField] float _uprightDownForceUpsideDownThreshold = 0.45f;
        [SerializeField] float _uprightDownForceLoweredPointDistance = 2f;
        [SerializeField] float _airYawAcceleration = 20f;
        [SerializeField] float _airPitchAcceleration = 20f;
        [SerializeField] float _airRollAcceleration = 20f;
        [SerializeField] float _terminalVelocity = 100f;
        [SerializeField] float _playerHitImpulseFactor = 1f;
        [SerializeField] float _playerHitAddedImpulse = 0.1f;
        [Tooltip("This value is multiplied by the hit force to decide how long tire friction will be reduced")]
        [SerializeField] float _playerHitSlipTimeFactor = 1f;
        [Tooltip("This value is always added to the car's vulnerability. The higher the value, the less the other factors have any impact")]
        [SerializeField] float _playerHitVulnerabilityConstant = 2f;

        [Header("Ball kick")]
        [SerializeField] AnimationCurve _speedToBallHitSpeedCurve = AnimationCurveUtility.Linear01();
        [SerializeField, Range(0, 1)] float _normalizedBallKickOppositeForce = 0.5f;

        [Header("Boost")]
        [SerializeField] float _boostWarmUpDuration = 0.4f;
        [SerializeField] float _boostForce = 2000f;
        [SerializeField, CurveRange(0, 0, 1, 1)] AnimationCurve _boostCurve = AnimationCurveUtility.Linear01();

        [Header("Internal references")]
        [SerializeField, Required] PlayerInput _playerInput = null;
        [SerializeField] List<Collider> _wheelColliders = new List<Collider>(WheelCount);
        [Tooltip("Front wheels are used for turning")]
        [SerializeField] List<Collider> _frontWheelColliders = new List<Collider>();
        [SerializeField, Required] PlayerModel _playerModel = null;
        [SerializeField, Required] Rigidbody _rigidbody = null;
        // TODO: Ideally player would not be aware of CarGroup, but this is
        // required because of the way PlayerInputManager is used currently.
        [SerializeField] CarGroup _carGroup = null;
        [SerializeField] LaunchDomeModel _launchDomeModel = null;
        [SerializeField] SphereCollider _launchDomeCollider = null;

        Dictionary<Collider, WheelContactInfo> _wheelContactInfos;
        double _lastBodyGroundContactTime;
        bool BodyGrounded => Time.fixedTime - _lastBodyGroundContactTime < Time.fixedDeltaTime * 1.5f;
        Vector3 _bodyGroundNormal;
        bool _wheelsGrounded;
        public bool WheelsGrounded => _wheelsGrounded;
        Vector3 _wheelsGroundNormal;
        public Vector3 WheelsGroundNormal => _wheelsGroundNormal;
        double _jumpSquatStartTime = double.NegativeInfinity;
        bool InJumpSquat => Time.timeAsDouble < _jumpSquatStartTime + _jumpSquatDuration;
        bool _prevInJumpSquat = false;
        double _boostStartTime = double.PositiveInfinity;
        bool _prevPivot = false;
        Vector3 _pivotStartDirection;
        float _pivotStartSpeed;
        Inputs _inputs;
        public Inputs Inputs => _inputs;
        float _stamina;
        bool _isGassedOut = false;
        bool _diveUsed = false;
        double _pulseChargeStartTime = double.PositiveInfinity;
        float _pulseCharge = 0;
        double _lastBoostPressTime = double.NegativeInfinity;
        bool _boostPressValid = false;
        double _lastPlayerCollisionTime = double.NegativeInfinity;
        double _slipStartTime = -1;
        double _slipEndTime = 0;
        float _launchChargeAmount = 0f;
        float _launchDomeRemainingDuration = 0f;
        private float _launchCount;

        InputAction _jumpAction = null;
        InputAction _diveAction = null;
        InputAction _movementAction = null;
        InputAction _lookAction = null;
        InputAction _accelerateAction = null;
        InputAction _brakeAction = null;
        InputAction _fastFallAction = null;
        InputAction _pivotAction = null;
        InputAction _boostAction = null;
        InputAction _recoveryRollAction = null;
        InputAction _changeAirRotationAxisAction = null;
        InputAction _pulseAction = null;
        InputAction _crouchAction = null;
        InputAction _launchAction = null;

        static Collider[] _collidersTemp = new Collider[16];

        // -- Public interface

        public bool IsGassedOut => _isGassedOut;
        public float Stamina => _stamina;
        public CarGroup CarGroup => _carGroup;

        public bool IsInputEnabled { get; set; }

        public PlayerModel Appearance => _playerModel;

        public void Spawn(Vector3 position, Quaternion rotation) {
            // Disable boost effect to clear trail renderers.
            Appearance.IsBoostEffectActive = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.position = position;
            // HACK ALERT: Need to set transform.position here because the actual root
            // group object can move which interacts badly with just setting
            // rigidbody.position for some reason.
            transform.position = position;
            _rigidbody.rotation = rotation;
            _stamina = StaminaConfig.InitialStamina;
            _isGassedOut = false;
            _launchChargeAmount = 0f;
        }

        // -- Unity events

        private void Awake() {
            // Center of Mass exactly at the pivot point
            _rigidbody.centerOfMass = Vector3.zero;

            _wheelContactInfos = new Dictionary<Collider, WheelContactInfo>();
            foreach (var wheelCollider in _wheelColliders) {
                _wheelContactInfos.Add(wheelCollider, new WheelContactInfo());
            }
        }

        void Start() {
            // Configure input actions (must happen in `Start` so that config
            // can be assigned either by GameManager or inspector.)
            // _playerInput.actions = CarConfig.InputActions;
            var map = _playerInput.currentActionMap;
            _jumpAction = map.FindAction("Jump", true);
            _diveAction = map.FindAction("Dive", true);
            _movementAction = map.FindAction("Move", true);
            _lookAction = map.FindAction("Look", true);
            _accelerateAction = map.FindAction("Accelerate", true);
            _brakeAction = map.FindAction("Brake", true);
            _fastFallAction = map.FindAction("FastFall", true);
            _pivotAction = map.FindAction("Pivot", true);
            _boostAction = map.FindAction("Boost", true);
            _recoveryRollAction = map.FindAction("RecoveryRoll", true);
            _changeAirRotationAxisAction = map.FindAction("ChangeAirRotationAxis", true);
            _pulseAction = map.FindAction("Pulse", true);
            _crouchAction = map.FindAction("Crouch", true);
            _launchAction = map.FindAction("Launch", true);
        }

        Inputs GetInputs() {
            if (!IsInputEnabled) {
                return new Inputs();
            }

            var result = new Inputs {
                Move = _movementAction.ReadValue<Vector2>(),
                Look = _lookAction.ReadValue<Vector2>(),
                Accelerate = _accelerateAction.ReadValue<float>(),
                Brake = _brakeAction.ReadValue<float>(),
                JumpPress = _jumpAction.WasPressedThisFrame(),
                JumpHold = _jumpAction.IsPressed(),
                Dive = CarConfig.DiveOnRelease
                    ? _diveAction.WasReleasedThisFrame()
                    : _diveAction.WasPressedThisFrame(),
                FastFall = _fastFallAction.IsPressed(),
                Pivot = _pivotAction.IsPressed(),
                BoostHold = _boostAction.IsPressed(),
                BoostPress = _boostAction.WasPressedThisFrame(),
                RecoveryRoll = _recoveryRollAction.IsPressed(),
                ChangeAirRotationAxis = _changeAirRotationAxisAction.IsPressed(),
                Pulse = _pulseAction.IsPressed(),
                Crouch = _crouchAction.IsPressed(),
                Launch = _launchAction.IsPressed(),
            };

            if (CarConfig.AutomaticAcceleration && _inputs.Brake == 0) {
                result.Accelerate = 1f;
            }

            return result;
        }

        private void FixedUpdate() {
            _inputs = GetInputs();

            var deltaTime = Time.deltaTime;

            // Check for double tapping the boost button (to make boost valid)
            if (CarConfig.DoubleTapToBoost) {
                if (_inputs.BoostPress) {
                    _boostPressValid = (Time.timeAsDouble <= _lastBoostPressTime + CarConfig.DoubleTapToBoostMaxDelay);
                    _lastBoostPressTime = Time.timeAsDouble;
                }
            }
            else {
                _boostPressValid = true;
            }

            bool didUseStamina = false;
            bool TryUseStamina(float amount) {
                Debug.Assert(amount >= 0, "Cannot use negative stamina");

                didUseStamina = true;

                if (_isGassedOut) {
                    return false;
                }

                _stamina = Mathf.Max(_stamina - amount, 0);

                if (_stamina > 0) {
                    return true;
                }

                // Too little stamina, we still perform the action, but it
                // results in a gas out.
                _isGassedOut = true;
                return true;
            }

            // Calculate normalized target speed (to be used for acceleration and other calcs)
            var normalizedTargetSpeed = Mathf.Max(
                (_inputs.Accelerate - _inputs.Brake),
                Mathf.Min(_inputs.Move.y, 0)
            );

            // Pulse
            if (_inputs.Pulse) {
                if (!IsChargingPulse) {
                    StartChargingPulse();
                }
                // Calculate how much stamina can be used this frame (without overcharging).
                var staminaDelta = Mathf.Clamp(
                    StaminaConfig.PulseStaminaRate * deltaTime,
                    0,
                    CarConfig.MaxPulseCharge - _pulseCharge
                );
                if (TryUseStamina(staminaDelta)) {
                    _pulseCharge += staminaDelta;
                    _playerModel.SetPulseCharge(_pulseCharge / CarConfig.MaxPulseCharge);
                }
            } else if (IsChargingPulse) {
                Pulse();
            }

            // Boost
            bool isBoosting;
            if (
                (CarConfig.AllowBoostWhilePivoting || !_inputs.Pivot) &&
                _inputs.BoostHold &&
                _boostPressValid &&
                TryUseStamina(StaminaConfig.BoostStaminaRate * deltaTime)
            ) {
                if (double.IsPositiveInfinity(_boostStartTime)) {
                    _boostStartTime = Time.timeAsDouble;
                }
                isBoosting = true;
            } else {
                _boostStartTime = double.PositiveInfinity;
                isBoosting = false;
            }
            if (isBoosting) {
                var boostT = (Time.timeAsDouble - _boostStartTime) / _boostWarmUpDuration;
                var normalizedBoost = _boostCurve.Evaluate((float) boostT);
                _rigidbody.AddForceAtPosition(
                    transform.forward * normalizedBoost * _boostForce * deltaTime,
                    transform.position,
                    ForceMode.Acceleration
                );
            }
            _playerModel.IsBoostEffectActive = isBoosting;

            // Max angular velocity (update here so we can tune in editor)
            _rigidbody.maxAngularVelocity = _maxAngularVelocity;
            

            // Get ground normal from wheels
            var totalWheelGroundNormal = Vector3.zero;
            var groundedWheelCount = 0;
            foreach (var wheelContactInfo in _wheelContactInfos) {
                if (wheelContactInfo.Value.Grounded) {
                    totalWheelGroundNormal += wheelContactInfo.Value.GroundNormal;
                    groundedWheelCount++;
                }
            }
            _wheelsGroundNormal = totalWheelGroundNormal.normalized;

            _wheelsGrounded = groundedWheelCount >= 3;
            if (_wheelsGrounded) {
                _diveUsed = false;
            }
            
            if (_wheelsGrounded)
            {
                var velocity = _rigidbody.velocity * Mathf.Pow(CarConfig.GroundedFriction, deltaTime * 60.0f);
                _rigidbody.velocity = velocity;
            }
            else
            {
                var velocity = _rigidbody.velocity;

                var clampedVelocity = velocity.ClampMagnitude(CarConfig.AirTerminalVelocity);
                
                _rigidbody.velocity = Vector3.Lerp(clampedVelocity, velocity, Mathf.Pow(CarConfig.AirFrictionAtTerminalVelocity, deltaTime * 60.0f));
                
            }

            var pivot = _inputs.Pivot && _wheelsGrounded;

            var currentSpeed = Vector3.Dot(_rigidbody.velocity, transform.forward);

            // Wall gravity
            if (groundedWheelCount > 0) {
                var wallGravity = _wheelsGroundNormal * Physics.gravity.y;
                var counterGravity = Vector3.up * -Physics.gravity.y;
                _rigidbody.AddForce(wallGravity + counterGravity, ForceMode.Acceleration);
            }

            // Angular drag
            _rigidbody.angularDrag = pivot ? _pivotAngularDrag : _regularAngularDrag;

            // Turn with front wheels
            var wheelTurnDegrees = _wheelTurnFactorBySpeed.Evaluate(Mathf.Abs(currentSpeed) / _drivingSpeed) * _maxWheelTurnDegrees;
            foreach (var wheel in _frontWheelColliders) {
                wheel.transform.localEulerAngles = new Vector3(0f, _inputs.Move.x * wheelTurnDegrees, 0f);
            }

            // Friction with each wheel
            if (_wheelsGrounded && !pivot) {
                foreach (var wheel in _wheelColliders) {
                    var normal = _wheelContactInfos[wheel].Grounded ? _wheelContactInfos[wheel].GroundNormal : _wheelsGroundNormal;
                    // Find the velocity with which each wheel is travelling along the ground
                    var currentWorldVelocity = _rigidbody.GetPointVelocity(wheel.transform.position);
                    currentWorldVelocity = Vector3.ProjectOnPlane(currentWorldVelocity, normal);
                    var currentLocalVelocity = wheel.transform.InverseTransformVector(currentWorldVelocity);
                    var wheelForwardsBackwardsWorldDirection = Vector3.Dot(currentWorldVelocity, wheel.transform.forward) > 0f ? wheel.transform.forward : -wheel.transform.forward;
                    wheelForwardsBackwardsWorldDirection = Vector3.ProjectOnPlane(wheelForwardsBackwardsWorldDirection, normal).normalized;

                    var retainedSpeed = currentLocalVelocity.magnitude * Mathf.InverseLerp(0f, _fullSpeedRetainDotProduct, Vector3.Dot(wheelForwardsBackwardsWorldDirection, currentWorldVelocity.normalized));

                    // Find the target local velocity for this wheel
                    var targetLocalVelocity = wheel.transform.InverseTransformVector(wheelForwardsBackwardsWorldDirection) * retainedSpeed;
                    // Required impulse to reach the target velocity
                    var localDifference = targetLocalVelocity - currentLocalVelocity;

                    // Variable friction based on "sidewaysness" (allows sliding sideways, but still tight steering)
                    var currentLocalDirection = currentLocalVelocity.normalized;
                    // sidewaysness is 1 when sliding sideways, 0 when straight ahead
                    var sidewaysness = Mathf.Acos(Mathf.Abs(currentLocalDirection.z)) / (0.5f * Mathf.PI);
                    var friction = _sidewaysnessVsFriction.Evaluate(sidewaysness) * Mathf.InverseLerp(0f, (float)(_slipEndTime - _slipStartTime), (float)(Time.timeAsDouble - _slipStartTime));
                    var lerpAmount = MathUtility.OldLerpCoefficientToBetterLerpCoefficient(friction, 60f, Time.fixedDeltaTime);

                    // Total impulse
                    var localImpulse = lerpAmount * localDifference / WheelCount;
                    var worldImpulse = wheel.transform.TransformVector(localImpulse);

                    // Force point is the wheel position, but raised to the center of mass
                    var wheelOffset = wheel.transform.position - transform.position;
                    var projectedWheelOffset = Vector3.ProjectOnPlane(wheelOffset, transform.up);
                    var forcePoint = transform.position + projectedWheelOffset;
                    _rigidbody.AddForceAtPosition(worldImpulse, forcePoint, ForceMode.VelocityChange);
                    
                }
            }

            // Start pivot
            if (pivot && !_prevPivot) {
                _pivotStartDirection = transform.forward;
                _pivotStartSpeed = _rigidbody.velocity.magnitude;
            }

            // Pivot drag
            if (pivot) {
                var drag = _pivotFriction * -_rigidbody.velocity;
                _rigidbody.AddForce(drag, ForceMode.Acceleration);
            }

            // Pivot turn
            if (pivot) {
                // Pivot 180 (if holding down on the stick)
                if (_inputs.Move.y < -0.6f) {
                    var targetDirection = Vector3.ProjectOnPlane(-_pivotStartDirection, _wheelsGroundNormal).normalized;
                    if (_rigidbody.velocity.sqrMagnitude > 0.01f) {
                        targetDirection = -_rigidbody.velocity.normalized;
                    }
                    var angle = Vector3.SignedAngle(transform.forward, targetDirection, transform.up);
                    if (Mathf.Abs(angle) < 3f) {
                        _rigidbody.angularVelocity = Vector3.zero;
                    }
                    else {
                        var yawAmount = Mathf.Sign(angle) * _pivot180GroundYawAcceleration;
                        _rigidbody.AddTorque(transform.up * yawAmount, ForceMode.Acceleration);
                    }
                }
                // Normal pivot (tank controls)
                else {
                    var yawAmount = _inputs.Move.x * _pivotGroundYawAcceleration;
                    _rigidbody.AddTorque(transform.up * yawAmount, ForceMode.Acceleration);
                }
            }

            // End pivot
            if (_wheelsGrounded && !pivot && _prevPivot) {
                // Cap angular velocity
                if (_rigidbody.angularVelocity.magnitude > _endPivotMaxAngularVelocity) {
                    _rigidbody.angularVelocity = _rigidbody.angularVelocity.normalized * _endPivotMaxAngularVelocity;
                }
                // Redirect velocity in facing direction if we're accelerating
                if (normalizedTargetSpeed > 0f) {
                    var angleToForwards = Mathf.Abs(Vector3.SignedAngle(_rigidbody.velocity, transform.forward, transform.up));
                    var forwardsness = Mathf.InverseLerp(180f, 0f, angleToForwards);
                    var redirectFactor = Mathf.Lerp(_pivotBackwardsRedirectFactor, _pivotForwardRedirectFactor, forwardsness);

                    _rigidbody.velocity = transform.forward * _pivotStartSpeed * redirectFactor;
                }
            }

            // Acceleration force
            if (_wheelsGrounded && !pivot) {
                var targetSpeed = _drivingSpeed * normalizedTargetSpeed;

                var acceleration = 0f;
                if (targetSpeed != 0f) {
                    var overspeedRatio = currentSpeed / targetSpeed; 
                    acceleration = _drivingAccelerationCurve.Evaluate(overspeedRatio) * _maxDrivingAcceleration * Mathf.Sign(targetSpeed);
                }
                else {
                    acceleration = -(currentSpeed / _drivingSpeed) * _maxDrivingAcceleration;
                }
                var force = transform.forward * acceleration;
                Debug.DrawRay(transform.position, force, Color.red, 0, false);
                _rigidbody.AddForce(force, ForceMode.Acceleration);
            }

            // Roll upright
            var applyUprightRoll = false;
            var rollUprightGroundNormal = Vector3.up;
            if (CarConfig.AutomaticRecoveryRoll || _inputs.RecoveryRoll) {
                if (groundedWheelCount > 0 && groundedWheelCount < 3) {
                    applyUprightRoll = true;
                    rollUprightGroundNormal = _wheelsGroundNormal;
                }
                else if (groundedWheelCount == 0 && BodyGrounded) {
                    applyUprightRoll = true;
                    rollUprightGroundNormal = _bodyGroundNormal;
                }
            }

            if (applyUprightRoll) {
                var noseUpOrDownAmount = Mathf.Abs(Vector3.Dot(transform.forward, rollUprightGroundNormal));
                var angle = Vector3.SignedAngle(transform.up, rollUprightGroundNormal, transform.forward);

                var rollRight = angle < 0f;
                var upsideDownAmount = MathUtility.Remap(Mathf.Abs(angle), 0f, 180f, 0f, 1f);
                var uprightTorque = upsideDownAmount * transform.forward * (rollRight ? -_uprightRollTorque : _uprightRollTorque) * (1f - noseUpOrDownAmount);
                _rigidbody.AddTorque(uprightTorque, ForceMode.Acceleration);
                // If we're upright enough, slam down into the ground for stability
                if (upsideDownAmount < _uprightDownForceUpsideDownThreshold) {
                    _rigidbody.AddForceAtPosition(-rollUprightGroundNormal * _uprightDownForce, transform.position - rollUprightGroundNormal * _uprightDownForceLoweredPointDistance, ForceMode.Acceleration);
                }
            }

            // Jump
            if (_wheelsGrounded && _inputs.JumpPress && !InJumpSquat) {
                _jumpSquatStartTime = Time.timeAsDouble;
            }
            if (_prevInJumpSquat && !InJumpSquat) {
                var addedVelocity = _wheelsGroundNormal * (_inputs.JumpHold ? _bigJumpSpeed : _smallJumpSpeed);
                _rigidbody.AddForce(addedVelocity, ForceMode.VelocityChange);
            }

            // Fast fall
            // TODO: consider only applying the force in the air, and preventing it during a small window after jumping
            if (_inputs.FastFall && _rigidbody.velocity.y > -_fastFallMaxSpeed) {
                var addedGravity = Vector3.down * _fastFallAcceleration;
                _rigidbody.AddForce(addedGravity, ForceMode.Acceleration);
            }

            // Dive
            if (!_wheelsGrounded && _inputs.Dive && !_diveUsed) {
                // Retain our velocity that's already in this direction
                var forwardVelocityDot = Vector3.Dot(_rigidbody.velocity, transform.forward);
                forwardVelocityDot = Mathf.Max(forwardVelocityDot, 0f);
                var orthogonalVelocity = Vector3.ProjectOnPlane(_rigidbody.velocity, transform.forward);
                _rigidbody.velocity = transform.forward * (forwardVelocityDot + _diveSpeed) + orthogonalVelocity * _diveOrthogonalVelocityRetention;
                _diveUsed = true;
            }

            _launchCount = (_launchCount + deltaTime * CarConfig.LaunchRechargeSpeed).Clamp(0, CarConfig.LaunchMaxCount.Max(1.0f));
            
            // Launch
            if (_inputs.Launch && _launchCount >= 1) {
                
                if (_launchChargeAmount <= 0.0f)
                {
                    _launchDomeModel.TriggerChargeStart();
                }
                
                // How much charge are we trying to gain this frame?
                var chargeDelta = Time.deltaTime / CarConfig.LaunchChargeDuration;
                // Can't go over charge = 1
                chargeDelta = Mathf.Min(chargeDelta, 1f - _launchChargeAmount);

                // how much stamina to deduct this frame
                var staminaCost = 0f;
                if (_launchChargeAmount == 0f) {
                    staminaCost = CarConfig.LaunchMinStaminaCost;
                }
                else {
                    staminaCost = chargeDelta * (CarConfig.LaunchMaxStaminaCost - CarConfig.LaunchMinStaminaCost);
                }

                // Only increase charge if we have the required stamina
                if (TryUseStamina(staminaCost)) {
                    _launchChargeAmount = _launchChargeAmount + chargeDelta;
                }

                _launchDomeModel.UpdateDome(_launchChargeAmount);
            }
            
            if (!_inputs.Launch && _launchChargeAmount > 0f)
            {
                _launchCount = (_launchCount - 1.0f).Max(0.0f);
                
                // Lock onto ball if close enough
                var ball = FindObjectOfType<Ball>();
                if (Vector3.Distance(transform.position, ball.transform.position) < CarConfig.LaunchBallLockDistance && _launchChargeAmount > 0.99f) {
                    transform.LookAt(ball.transform.position, Vector3.up);
                }

                var launchForce = _rigidbody.rotation * Vector3.forward * Mathf.Lerp(CarConfig.LaunchMinSpeed,
                    CarConfig.LaunchMaxSpeed, _launchChargeAmount);

                _rigidbody.AddForce(launchForce, ForceMode.Impulse);
                _launchDomeRemainingDuration = Mathf.Lerp(CarConfig.LaunchDomeMinDuration, CarConfig.LaunchDomeMaxDuration, _launchChargeAmount);
              
                _launchDomeModel.TriggerLaunch();

                _launchChargeAmount = 0f;
            }
            

            // Launch Dome, set visible
            if (_launchDomeRemainingDuration > 0f) {
                _launchDomeCollider.gameObject.SetActive(true);
            }
            else {
                _launchDomeCollider.gameObject.SetActive(false);
            }
            _launchDomeRemainingDuration -= Time.deltaTime;

            // Air control
            if (!_wheelsGrounded) {
                // Pitch
                var pitchAmount = _inputs.Move.y * _airPitchAcceleration;
                _rigidbody.AddTorque(transform.right * pitchAmount, ForceMode.Acceleration);

                // Yaw and roll
                var axisType = _inputs.ChangeAirRotationAxis
                    ? CarConfig.AlternateRotationAxis
                    : CarConfig.DefaultRotationAxis;

                var (axis, acceleration) = axisType switch {
                    CarRotationAxis.Yaw => (transform.up, _airYawAcceleration),
                    CarRotationAxis.Roll => (-transform.forward, _airRollAcceleration),
                    _ => throw DebugUtility.NoMatch(axisType)
                };

                var amount = _inputs.Move.x * acceleration;
                _rigidbody.AddTorque(axis * amount, ForceMode.Acceleration);
            }

            var unused = _terminalVelocity;
            // Clamp terminal velocity (primarily to limit aerial speed).
            // _rigidbody.velocity = Vector3.ClampMagnitude(
            //     _rigidbody.velocity,
            //     _terminalVelocity
            // );

            // Record "previous" variables
            _prevInJumpSquat = InJumpSquat;
            _prevPivot = pivot;

            // Update stamina.
            if (!didUseStamina) {
                var rechargeRate = _stamina < StaminaConfig.LowStaminaLevel
                    ? StaminaConfig.LowStaminaRechargeRate
                    : StaminaConfig.NormalStaminaRechargeRate;
                _stamina = Mathf.Min(
                    _stamina + rechargeRate * Time.deltaTime,
                    StaminaConfig.MaxStamina
                );
                if (_isGassedOut && _stamina == StaminaConfig.MaxStamina) {
                    _isGassedOut = false;
                }
            }
        }

        private void Update() {
            UpdatePlayerModel();
        }

#if UNITY_EDITOR
        void OnDrawGizmos() {
            var style = new GUIStyle {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                normal = {
                  textColor = Color.red
                }
            };
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Disabled;
            Handles.Label(
                transform.position + Vector3.up * 1,
                $"Stamina: {_stamina.ToString("N0")}",
                style
            );

            if (IsChargingPulse) {
                DrawPulseGizmos();
            }
        }

        void OnDrawGizmosSelected() {
            if (!IsChargingPulse) DrawPulseGizmos();
        }

        void DrawPulseGizmos() {
            var color = Color.magenta;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(
                transform.position,
                CarConfig.MinPulseRadius
            );
            Gizmos.color = color.WithAlpha(0.6f);
            Gizmos.DrawWireSphere(
                transform.position,
                CarConfig.MaxPulseRadius
            );
        }
#endif

        // -- General methods

        void UpdatePlayerModel() {
            // TODO: use actual wheel turn degrees, not input
            _playerModel.SetSteerAmount(_inputs.Pivot ? 0 : _inputs.Move.x);
            _playerModel.IsPivoting = _inputs.Pivot;
            _playerModel.IsCrouching = _inputs.Crouch;
        }

        // -- Wheel collision detection

        struct WheelContactInfo {
            public double LastGroundedTime;
            public Vector3 GroundNormal;
            public Vector3 Impulse;
            // Collision events are after fixedupdate in the physics cycle, so make sure we include collisions from last frame (only)
            public bool Grounded => Time.fixedTimeAsDouble - LastGroundedTime < Time.fixedDeltaTime * 1.5f;

            public WheelContactInfo(double lastGroundedTime, Vector3 groundNormal, Vector3 impulse) {
                LastGroundedTime = lastGroundedTime;
                GroundNormal = groundNormal;
                Impulse = impulse;
            }
        }

        void OnCollisionStay(Collision collision) {
            var isWheelCollision = false;
            foreach (var contact in collision.contacts) {
                if (_wheelColliders.Contains(contact.thisCollider)) {
                    RegisterGroundWheelCollision(contact, collision);
                    isWheelCollision = true;
                }
            }

            if (!isWheelCollision) {
                _lastBodyGroundContactTime = Time.fixedTimeAsDouble;
                _bodyGroundNormal = collision.contacts[0].normal;
            }
        }

        void RegisterGroundWheelCollision(ContactPoint contactPoint, Collision collision) {
            var info = new WheelContactInfo(Time.fixedTimeAsDouble, contactPoint.normal, collision.impulse);
            var collider = contactPoint.thisCollider;
            var prevInfo = _wheelContactInfos[collider];
            // Store this info if it's newer than what was previously stored, or higher impulse.
            // For some reason multiple collisions are registered each frame, one with an impulse and one without.
            if (info.LastGroundedTime > prevInfo.LastGroundedTime || info.Impulse.sqrMagnitude > prevInfo.Impulse.sqrMagnitude) {
                _wheelContactInfos[collider] = info;
            }
        }

        // -- Ball trigger enter --

        float TiltHitAngle(float tilt) {
            if (tilt > 0) {
                return Mathf.Lerp(
                    0,
                    CarConfig.LowTiltKickAngle,
                    tilt
                );
            }
            return Mathf.Lerp(
                0,
                CarConfig.HighTiltKickAngle,
                -tilt
            );
        }

        Quaternion TiltHitRotationAdjustment(float tilt) =>
            Quaternion.AngleAxis(TiltHitAngle(tilt), -transform.right);

        void OnTriggerEnter(Collider collider) {
            if (collider.GetComponentInParent<Ball>() != null) {
                // HandleBallCollision gets called from the ball for now, to determine which of the car colliders was used
            }
            else {
                var otherPlayer = collider.GetComponentInParent<Player>();
                if (otherPlayer != null) {
                    HandlePlayerOnPlayerCollision(this, otherPlayer);
                }
            }
        }

        static void HandlePlayerOnPlayerCollision(Player p1, Player p2) {
            // If these two players have already collided this frame, they almost certainly collided with each other. Don't double handle the collision. 
            if (p1._lastPlayerCollisionTime == Time.timeAsDouble && p2._lastPlayerCollisionTime == Time.timeAsDouble) {
                return;
            }

            // Impulse direction is between the two object centres, from p1 to p2
            var direction = Vector3.Normalize(p2._rigidbody.position - p1._rigidbody.position);

            // Magnitude of relative velocity in the impulse direction
            var speed = Vector3.Dot(
                p1._rigidbody.velocity - p2._rigidbody.velocity,
                direction
            );

            // If the cars are moving away from each other, don't do anything
            if (speed < 0f) {
                return;
            }

            // Assume the same _playerHitImpulseFactor for both players.
            if (p1._playerHitImpulseFactor != p2._playerHitImpulseFactor) {
                Debug.LogWarning("Colliding cars have different values for _playerHitImpulseFactor. This case is unhandled.");
            }
            var hitStrength = speed * p1._playerHitImpulseFactor + p1._playerHitAddedImpulse;

            // Calculate relative vulnerabilities of the cars
            var p1SidewaysnessOfHit = 1f - Mathf.Abs(Vector3.Dot(p1.transform.forward, direction));
            var p2SidewaysnessOfHit = 1f - Mathf.Abs(Vector3.Dot(p2.transform.forward, direction));

            var p1Vulnerability = p1SidewaysnessOfHit + p1._playerHitVulnerabilityConstant;
            var p2Vulnerability = p2SidewaysnessOfHit + p2._playerHitVulnerabilityConstant;

            var totalVulnerability = p1Vulnerability + p2Vulnerability;
            var p1HitStrength = 2f * hitStrength * p1Vulnerability / totalVulnerability;
            var p2HitStrength = 2f * hitStrength * p2Vulnerability / totalVulnerability;

            p1._rigidbody.AddForce(-direction * p1HitStrength, ForceMode.VelocityChange);
            p2._rigidbody.AddForce(direction * p2HitStrength, ForceMode.VelocityChange);

            var p1SlipDuration = p1HitStrength * p1._playerHitSlipTimeFactor;
            var p2SlipDuration = p2HitStrength * p2._playerHitSlipTimeFactor;
            p1._slipStartTime = Time.timeAsDouble;
            p1._slipEndTime = Time.timeAsDouble + p1SlipDuration;
            p2._slipStartTime = Time.timeAsDouble;
            p2._slipEndTime = Time.timeAsDouble + p2SlipDuration;

            p1._lastPlayerCollisionTime = Time.timeAsDouble;
            p2._lastPlayerCollisionTime = Time.timeAsDouble;

            //Debug.Log($"fixedtime: {Time.fixedTime}, 1st car, hit strength: {p1HitStrength}, slip duration: {p1SlipDuration}");
            //Debug.Log($"fixedtime: {Time.fixedTime}, 2nd car, hit strength: {p2HitStrength}, slip duration: {p2SlipDuration}");
        }

        public void HandleBallCollision(Collider carCollider, Collider ballCollider, Ball ball) {
            if (!ballCollider.TryGetComponent(out Rigidbody ballRigidbody)) {
                Debug.LogError($"Detected ball object '{ballCollider.name}' did not have a {nameof(Rigidbody)}!", ballCollider);
                return;
            }

            // Detect if it hit the Launch Dome and reject if outside of dome angle
            if (carCollider == (Collider)_launchDomeCollider) {
                var toBall = ballCollider.transform.position - carCollider.transform.position;
                var angle = Vector3.Angle(toBall, transform.forward);
                // if (angle > CarConfig.LaunchDomeHalfAngle) {
                //     return;
                // }
            }

            // Kick ball in direction from center of player to ball.
            var ballDirection = Vector3.Normalize(ballRigidbody.position - _rigidbody.position);

            // Ball speed is the component of the relative velocity in the
            // direction of the ball.
            var speed = Vector3.Dot(
                _rigidbody.velocity - ballRigidbody.velocity,
                ballDirection
            );

            // Add spin if pivoting.
            if (_prevPivot) {
                speed += Mathf.Abs(_rigidbody.angularVelocity.y) / 360f * CarConfig.PivotKickBallSpeed;
                ballRigidbody.AddTorque(
                    0,
                    -_rigidbody.angularVelocity.y * CarConfig.PivotKickBallSpinScale,
                    0,
                    ForceMode.VelocityChange
                );
            }

            // Adjust kick direction to hilt.

            var kickDirection = ballDirection;
            
            if (_inputs.Crouch)
            {
                
            }
            else if(_wheelsGrounded && ballRigidbody.position.y < (ball.Radius * 1.25f))
            {
                var magnitude = ballDirection.magnitude;
                kickDirection.y = 0.0f;
                kickDirection = kickDirection.normalized * magnitude;
            }

            // Map the kick velocity to hand tuned curve.
            var ballKickVelocity = kickDirection * _speedToBallHitSpeedCurve.Evaluate(speed);
            ballRigidbody.AddForce(ballKickVelocity, ForceMode.VelocityChange);

            // Use some percentage of this acceleration change to push the
            // player back as a result of the kick.
            _rigidbody.AddForce(-ballKickVelocity * _normalizedBallKickOppositeForce, ForceMode.VelocityChange);

#if UNITY_EDITOR
            {
                var maxCurveSpeed = _speedToBallHitSpeedCurve.MaxTime();
                var normalizedSpeed = speed / maxCurveSpeed;
                Debug.Log($"Kick speed {speed} ({normalizedSpeed.ToString("P0")} of max)");
            }
#endif
        }

        // -- Pulse --

        bool IsChargingPulse => _pulseChargeStartTime < Time.timeAsDouble;

        void StartChargingPulse() {
            Debug.Assert(!IsChargingPulse, "Already charging pulse");
            _pulseChargeStartTime = Time.timeAsDouble;
        }

        void Pulse() {
            Debug.Assert(IsChargingPulse);
            var count = Physics.OverlapSphereNonAlloc(
                transform.position,
                CarConfig.MaxPulseRadius,
                _collidersTemp,
                CarConfig.PulseLayerMask
            );
            for (var i = 0; i < count; i++) {
                var collider = _collidersTemp[i];
                var offset = collider.transform.position - transform.position;
                var distanceT = Mathf.InverseLerp(
                    CarConfig.MinPulseRadius,
                    CarConfig.MaxPulseRadius,
                    offset.magnitude
                );
                var distanceScale = CarConfig.PulseDistanceCurve.Evaluate(distanceT);
                var chargeT = _pulseCharge / CarConfig.MaxPulseCharge;
                var chargeScale = CarConfig.PulseChargeCurve.Evaluate(chargeT);
                var speed = chargeScale * distanceScale * CarConfig.MaxPulseBallSpeed;
                Debug.Log($"d={offset.magnitude}, dt={distanceT}, c={_pulseCharge}, ct={chargeT}, => speed={speed}");
                _collidersTemp[i].attachedRigidbody.AddForce(
                    offset.normalized * speed,
                    ForceMode.VelocityChange
                );
            }
            _pulseChargeStartTime = double.PositiveInfinity;
            _pulseCharge = 0;
            _playerModel.ReleasePulse();
        }
    }
}
