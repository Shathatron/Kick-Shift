using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine;

namespace Moball {
    class Goal : MonoBehaviour {
        [ValidateInput(nameof(IsValidId), "Select a team ID")]
        [SerializeField] int _scoringTeamId = -1;
        [SerializeField, Required] BoxCollider _collider = default;
    	[SerializeField] UnityEvent<Goal> _onGoal = default;
    	[SerializeField] float _explosionRadius = 1000f;
    	[SerializeField] float _explosionForce = 1000f;
    	[SerializeField] float _explosionUpwardsModifier = 500f;
    	[SerializeField, Required] Transform _explosionCenter = default;

        public int ScoringTeamId => _scoringTeamId;

        public void Explode() {
            var colliders = Physics.OverlapSphere(
                transform.position,
                _explosionRadius
            );
            foreach (var collider in colliders) {
                var rigidbody = collider.gameObject.GetComponentInParent<Rigidbody>();
                if (rigidbody) {
                    rigidbody.AddExplosionForce(
                        _explosionForce,
                        _explosionCenter.position,
                        _explosionRadius,
                        _explosionUpwardsModifier
                    );
                }
            }
        }

        void Start() {
            var angles = _collider.transform.eulerAngles;
            if (angles.x % 90 != 0 || angles.y % 90 != 0 || angles.z % 90 != 0) {
                Debug.LogError($"{nameof(Goal)} must be axis aligned!", this);
            }
        }

        void OnTriggerStay(Collider other) {
    		if (other.TryGetComponent(out Ball ball)) {
    			var bounds = ball.GetBounds();
    	        if (
    	            _collider.bounds.Contains(bounds.min) &&
    				_collider.bounds.Contains(bounds.max)
    			) {
    				_onGoal.Invoke(this);
    			}
    		} else {
    			Debug.LogWarning($"Detected object with no {nameof(Ball)} component");
    		}
        }

        // -- Validators --

        static bool IsValidId(int value) => value >= 0;
    }
}