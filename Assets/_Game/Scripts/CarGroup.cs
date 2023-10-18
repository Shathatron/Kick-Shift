#nullable enable

using UnityEngine;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine.InputSystem;

namespace Moball {
    public class CarGroup : MonoBehaviour {
        [Header("Camera")]
        [Tooltip("Visible space around the ball")]
        [SerializeField] float _ballCameraTargetRadiusMargin = 0f;
        [SerializeField] float _ballCameraTargetWeight = 1f;

        [Header("References")]
        [SerializeField, Required] Camera _camera = default!;
        [SerializeField, Required] CinemachineVirtualCameraBase _fixedAngleCarAndBallVirtualCamera = default!;
        [SerializeField, Required] CinemachineVirtualCameraBase _fixedAngleCarVirtualCamera = default!;
        [SerializeField, Required] CinemachineVirtualCameraBase _orbitCarVirtualCamera = default!;
        [SerializeField, Required] CinemachineVirtualCameraBase _orbitCarTargetBallVirtualCamera = default!;
        [SerializeField, Required] CinemachineTargetGroup _carAndBallTargetGroup = default!;
        [SerializeField, Required] StaminaIndicator _staminaIndicator = default!;
        [SerializeField, Required] Player _car = default!;
        [SerializeField, Required] PlayerInput _playerInput = default!;

        Quaternion _forwardCameraRotation;
        Quaternion _reversedCameraRotation;
        Ball? _ball;
        // Allow the camera behaviour to be overridden by other code, e.g. for goal scoring
        bool _forceFollowCar = false;
        int _cameraModeIndex;
        PlayerCameraMode CameraMode => _car.CarConfig.GetCameraMode(_cameraModeIndex);
        bool _isReversed = false;
        InputAction? _reverseCameraAction;
        InputAction? _cameraModeAction;

        // -- Unity events --

        void Start() {
            _cameraModeIndex = _car.CarConfig.InitialCameraModeIndex;
            RefreshCameraMode();

            var map = _playerInput.currentActionMap;
            _reverseCameraAction = map.FindAction("ReverseCamera", true);
            _cameraModeAction = map.FindAction("CameraMode", true);
        }

        void Update() {
            if (_staminaIndicator) {
                _staminaIndicator.SetPlayerPosition(_car.transform.position);
                _staminaIndicator.SetCameraFacing(_camera.transform.forward);
                var config = _car.StaminaConfig;
                _staminaIndicator.Set(
                    Mathf.InverseLerp(config.MinVisibleStamina, config.MaxStamina, _car.Stamina),
                    (_car.IsGassedOut, _car.Stamina) switch {
                        var (g, _) when g => StaminaStatus.GassedOut,
                        var (_, s) when s < config.MinVisibleStamina => StaminaStatus.Warning,
                        var (_, s) when s < config.LowStaminaLevel => StaminaStatus.Low,
                        _ => StaminaStatus.Normal
                    }
                );
            }
        }

        void FixedUpdate() {
            // NOTE: Input is processed on FixedUpdate.
            if (_reverseCameraAction!.WasPressedThisFrame()) {
                SetReversed(true);
            } else if (_reverseCameraAction!.WasReleasedThisFrame()) {
                SetReversed(false);
            }
            if (_cameraModeAction!.WasPressedThisFrame()) {
                _cameraModeIndex++;
                RefreshCameraMode();
            }
        }

        // -- Public interface --

        public void ConfigureCamera(int cameraLayer) {
            // Set up layers for split screen, so our Cinemachine Brain only considers our own Virtual Cameras.
            foreach (var virtualCamera in GetComponentsInChildren<CinemachineVirtualCameraBase>()) {
                virtualCamera.gameObject.layer = cameraLayer;
            }
            _camera.cullingMask |= 1 << cameraLayer;
            _staminaIndicator.Initialize(layer: cameraLayer);

            // Set up Follow cams
            foreach (var followCam in GetComponentsInChildren<OrbitCarCameraController>()) {
                followCam.Initialize(_car);
            }
        }

        public bool ForceFollowCar {
            get => _forceFollowCar;
            set {
                _forceFollowCar = value;
                RefreshCameraMode();
            }
        }

        public Ball? Ball {
            get => _ball;
            set {
                if (_ball != null) RemoveCameraTarget(_ball.CameraTarget);
                _ball = value;

                if (_ball != null) {
                    // Update ball on camera controllers
                    AddOrUpdateCameraTarget(
                                _ball.CameraTarget,
                                _ballCameraTargetWeight,
                                _ball.Radius + _ballCameraTargetRadiusMargin
                            );
                    _orbitCarTargetBallVirtualCamera.GetComponent<OrbitCarCameraController>().Ball = _ball; 
                }
            }
        }

        // -- Private implementation --

        void SetReversed(bool value) {
            if (_isReversed != value) {
                _isReversed = value;
                RefreshCameraRotation();
            }
        }

        void RefreshCameraRotation() {
            // TODO: reimplement reverse camera
            //_virtualCamera.transform.localRotation = _isReversed
            //    ? _reversedCameraRotation
            //    : _forwardCameraRotation;
        }

        void RefreshCameraMode() {
            // Zero out all camera priorities and just set the one we want active
            _fixedAngleCarAndBallVirtualCamera.Priority = 0;
            _fixedAngleCarVirtualCamera.Priority = 0;
            _orbitCarVirtualCamera.Priority = 0;
            _orbitCarTargetBallVirtualCamera.Priority = 0;

            switch ((_forceFollowCar, CameraMode)) {
                case (true, _):
                case (_, PlayerCameraMode.FixedAngleCar):
                    _fixedAngleCarVirtualCamera.Priority = 10;
                    break;
                case (_, PlayerCameraMode.FixedAngleCarAndBall):
                    _fixedAngleCarAndBallVirtualCamera.Priority = 10;
                    if (_ball == null) {
                        Debug.LogWarning("No ball to follow");
                    }
                    break;
                case (_, PlayerCameraMode.OrbitCar):
                    _orbitCarVirtualCamera.Priority = 10;
                    break;
                case (_, PlayerCameraMode.OrbitCarTargetBall):
                    _orbitCarTargetBallVirtualCamera.Priority = 10;
                    if (_ball == null) {
                        Debug.LogWarning("No ball to follow");
                    }
                    break;
                default:
                    DebugUtility.AssertNoMatch(CameraMode);
                    break;
            }
        }

        void RemoveCameraTarget(Transform target) {
            _carAndBallTargetGroup.RemoveMember(target);
        }

        void AddOrUpdateCameraTarget(Transform target, float weight, float radius) {
            RemoveCameraTarget(target);
            _carAndBallTargetGroup.AddMember(target, weight, radius);
        }
    }
}
