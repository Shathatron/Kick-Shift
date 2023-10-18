using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// A custom camera controller that controls a CinemachineVirtualCamera.
// Used when the camera is following the car from behind with player controlled orbiting

namespace Moball {
    [System.Serializable]
    public struct OrbitCamGroundAngleSetting {
        public float Pitch;
        public float Roll;
        // 0 = camera up is world up, 1 = camera up is ground normal
        [Range(0f, 1f)]
        public float CameraUpIsGroundNormal;

        public OrbitCamGroundAngleSetting(float pitch, float roll, float cameraUpIsGroundNormal) {
            Pitch = pitch;
            Roll = roll;
            CameraUpIsGroundNormal = cameraUpIsGroundNormal;
        }

        public static OrbitCamGroundAngleSetting operator *(OrbitCamGroundAngleSetting a, float b)
        => new OrbitCamGroundAngleSetting(a.Pitch * b, a.Roll * b, a.CameraUpIsGroundNormal * b);

        public static OrbitCamGroundAngleSetting operator +(OrbitCamGroundAngleSetting a, OrbitCamGroundAngleSetting b)
        => new OrbitCamGroundAngleSetting(a.Pitch + b.Pitch, a.Roll + b.Roll, a.CameraUpIsGroundNormal + b.CameraUpIsGroundNormal);
    }

    [System.Serializable]
    public struct OrbitCamGroundAngleSettingsGroup {
        public OrbitCamGroundAngleSetting Floor;
        public OrbitCamGroundAngleSetting Ceiling;
        public OrbitCamGroundAngleSetting WallUp;
        public OrbitCamGroundAngleSetting WallDown;
        public OrbitCamGroundAngleSetting WallSide;

        public OrbitCamGroundAngleSetting Blend(Vector3 groundNormal, Vector3 facingDirection) {
            var floorWeight = Mathf.InverseLerp(90f, 0f, Vector3.Angle(groundNormal, Vector3.up));
            var ceilingWeight = Mathf.InverseLerp(90f, 0f, Vector3.Angle(groundNormal, Vector3.down));
            var wallWeight = 1f - floorWeight - ceilingWeight;

            var wallBlend = new OrbitCamGroundAngleSetting();
            if (wallWeight > 0f) {
                var fullUprightGroundNormal = Vector3.ProjectOnPlane(groundNormal, Vector3.up);
                var fullUprightFacingDirection = Vector3.ProjectOnPlane(facingDirection, fullUprightGroundNormal);

                var wallUpWeight = Mathf.InverseLerp(90f, 0f, Vector3.Angle(fullUprightFacingDirection, Vector3.up));
                var wallDownWeight = Mathf.InverseLerp(90f, 0f, Vector3.Angle(fullUprightFacingDirection, Vector3.down));
                var wallSideWeight = 1f - wallUpWeight - wallDownWeight;

                var turnedRightOnWallSign = Mathf.Sign(Vector3.SignedAngle(Vector3.up, fullUprightFacingDirection, fullUprightGroundNormal));
                var wallSideModified = new OrbitCamGroundAngleSetting(WallSide.Pitch, WallSide.Roll * turnedRightOnWallSign, WallSide.CameraUpIsGroundNormal);

                wallBlend = wallSideModified * wallSideWeight + WallUp * wallUpWeight + WallDown * wallDownWeight;
            }

            return wallBlend * wallWeight + Floor * floorWeight + Ceiling * ceilingWeight;
        }
    }

    public class OrbitCarCameraController : MonoBehaviour {
        [SerializeField] CinemachineVirtualCamera _virtualCamera;
        [SerializeField] float _pivotHeightAboveCar = 2f;
        [SerializeField] float _cameraDistance = 4.5f;
        [SerializeField] float _autoTurnSpeed = 0.25f;
        [SerializeField] float _lookAroundTurnSpeed = 0.15f;
        [SerializeField] Vector2 _maxLookAroundDegrees = new Vector2(130f, 65f);
        [SerializeField] OrbitCamGroundAngleSettingsGroup _groundAngleSettingsGroup = default!;
        public Ball Ball = null;

        Player _player;

        Vector3 _pivotOffset = Vector3.zero;
        Vector3 _lookDirection = Vector3.forward;
        Vector3 _cameraUp = Vector3.up;
        // Use the stick to offset camera direction
        Vector2 _lookAroundOffsetAmount = Vector2.zero;

        public void Initialize(Player player) {
            _player = player;
            _lookDirection = _player.transform.forward;
        }

        void Update() {
            if (_player == null) {
                return;
            }

            // Default targets (also used for in the air)
            var targetLookDirection = _lookDirection;
            var targetPivotOffset = _pivotHeightAboveCar * Vector3.up;
            var targetCameraUp = Vector3.up;

            // If ball cam is active
            if (Ball != null) {
                targetLookDirection = (Ball.transform.position - _player.transform.position).normalized;
                targetPivotOffset = _pivotHeightAboveCar * Vector3.up;
                targetCameraUp = Vector3.up;
            }
            // Grounded non-ball cam
            else if (_player.WheelsGrounded) {
                var pitchPlane = Vector3.ProjectOnPlane(_player.transform.right, _player.WheelsGroundNormal).normalized;
                var carForwardOnGround = Vector3.ProjectOnPlane(_player.transform.forward, _player.WheelsGroundNormal).normalized;
                var pitchRoll = _groundAngleSettingsGroup.Blend(_player.WheelsGroundNormal, carForwardOnGround);

                targetLookDirection = Quaternion.AngleAxis(pitchRoll.Pitch, pitchPlane) * carForwardOnGround;
                targetPivotOffset = _pivotHeightAboveCar * _player.WheelsGroundNormal;

                targetCameraUp = Vector3.Slerp(Vector3.up, _player.WheelsGroundNormal, pitchRoll.CameraUpIsGroundNormal);
                targetCameraUp = Quaternion.AngleAxis(pitchRoll.Roll, targetLookDirection) * targetCameraUp;
            }

            // Interpolate yaw and pitch seperately, to prevent camera going over the top
            var lookDiffYaw = Vector3.SignedAngle(_lookDirection, targetLookDirection, targetCameraUp);
            var lookYawRotation = Quaternion.AngleAxis(lookDiffYaw, targetCameraUp);
            var afterLookYawRotation = lookYawRotation * _lookDirection;
            var lookPitchRollRotation = Quaternion.FromToRotation(afterLookYawRotation, targetLookDirection);
            _lookDirection = Quaternion.Slerp(Quaternion.identity, lookYawRotation, _autoTurnSpeed * Time.deltaTime) * _lookDirection;
            _lookDirection = Quaternion.Slerp(Quaternion.identity, lookPitchRollRotation, _autoTurnSpeed * Time.deltaTime) * _lookDirection;

            _pivotOffset = Vector3.Slerp(_pivotOffset, targetPivotOffset, _autoTurnSpeed * Time.deltaTime);
            _cameraUp = Vector3.Slerp(_cameraUp, targetCameraUp, _autoTurnSpeed * Time.deltaTime);

            // Camera control offset
            var inputs = _player.Inputs;
            _lookAroundOffsetAmount = Vector2.Lerp(_lookAroundOffsetAmount, inputs.Look, _lookAroundTurnSpeed * Time.deltaTime);

            var lookAroundLocalRight = -Vector3.Cross(targetLookDirection, Vector3.up);
            var lookAroundRotation
                = Quaternion.Euler(0f, _lookAroundOffsetAmount.x * _maxLookAroundDegrees.x * (_player.CarConfig.InvertCameraX ? -1f : 1f), 0f)
                * Quaternion.AngleAxis(_lookAroundOffsetAmount.y * _maxLookAroundDegrees.y * (_player.CarConfig.InvertCameraY ? 1f : -1f), lookAroundLocalRight);

            var forwardsRotation = Quaternion.LookRotation(_lookDirection, _cameraUp);
            var finalRotation = lookAroundRotation * forwardsRotation;
            var pos = _player.transform.position + _pivotOffset - (finalRotation * Vector3.forward * _cameraDistance);
            _virtualCamera.transform.position = pos;
            _virtualCamera.transform.rotation = finalRotation;
        }
    }

}
