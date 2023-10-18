using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

namespace Moball {
    class PlayerSpawnPoint : MonoBehaviour {
        [ValidateInput(nameof(IsValidId), "Select a team ID")]
        [SerializeField] int _id = -1;
        [ValidateInput(nameof(IsValidId), "Select a team ID")]
        [SerializeField] int _teamId = -1;

        public int Id => _id;
        public int TeamId => _teamId;
        public Vector3 SpawnPosition => transform.position;

        static bool IsValidId(int value) => value >= 0;

        static void FindAll(Transform root, List<PlayerSpawnPoint> results) =>
            root.GetComponentsInChildren<PlayerSpawnPoint>(results);
    }
}