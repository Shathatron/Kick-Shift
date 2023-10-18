using UnityEngine;
using NaughtyAttributes;

namespace Moball {
    public class Ball : MonoBehaviour {
        public BallConfig Config;

        [SerializeField, Required] SphereCollider _collider = default;
        [SerializeField, Required] Transform _cameraTarget = default;
        [SerializeField, Required] Rigidbody _rigidbody = default;
        [SerializeField, Required] Transform _dotRoot = default;
        [SerializeField] Color _airDotColor = Color.red;

        int _environmentLayer = -1;
        bool _isGrounded = false;
        Material _dotMaterial;
        Color _defaultDotColor;

        public float Radius =>
            _collider.radius *
            Vector3Utility.Max(_collider.transform.lossyScale);

        public Transform CameraTarget => _cameraTarget;

        public Bounds GetBounds() => _collider.bounds;

        public void Stop() {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        // -- Unity events --

        void Awake() {
            var dotRenderers = _dotRoot.GetComponentsInChildren<SpriteRenderer>();
            _dotMaterial = dotRenderers[0].material;
            _defaultDotColor = _dotMaterial.color;
            foreach (var renderer in dotRenderers) {
                renderer.sharedMaterial = _dotMaterial;
            }
        }

        void Start() {
            _environmentLayer = LayerMask.NameToLayer("Environment");
            transform.localScale = Config.Scale * Vector3.one;
        }

        void Update() {
            var position = transform.position;
            position.y /= 2;
            _cameraTarget.position = position;

            _dotMaterial.color = _isGrounded ? _defaultDotColor : _airDotColor;
        }

        void FixedUpdate() {
            // Calculate magnus effect
            //
            // NOTE: I think angular velocity needs to be inverted because Unity
            // stores rotations in counter-clockwise, but the equation expects
            // clockwise.
            var spinScale = _isGrounded ? Config.GroundSpinScale : Config.AirSpinScale;
            var magnusAcceleration = spinScale * Vector3.Cross(_rigidbody.velocity, -_rigidbody.angularVelocity);
            Debug.DrawRay(_rigidbody.position, magnusAcceleration / 10f, Color.yellow);
            _rigidbody.AddForce(
                magnusAcceleration * Time.deltaTime,
                ForceMode.Acceleration
            );

            // Gravity and mass.
            _rigidbody.AddForce(0, -Config.Gravity, 0, ForceMode.Acceleration);
            _rigidbody.mass = Config.Mass;
        }

        void OnCollisionEnter(Collision collsion) {
            if (collsion.gameObject.layer == _environmentLayer) {
                _isGrounded = true;
            }
        }

        void OnCollisionExit(Collision collsion) {
            if (collsion.gameObject.layer == _environmentLayer) {
                _isGrounded = false;
            }
        }
        void OnTriggerEnter(Collider collider) {
            var player = collider.GetComponentInParent<Player>();
            if (player != null) {
                player.HandleBallCollision(collider, _collider, this);
            }
        }

    }
}
