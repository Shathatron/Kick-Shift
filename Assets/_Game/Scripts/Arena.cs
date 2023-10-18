using UnityEngine;

namespace Moball {
    class Arena : MonoBehaviour {
        [System.Serializable]
        struct RendererMaterialId {
            public Renderer Renderer;
            public int Index;
        }
        [SerializeField] RendererMaterialId[] _team0Renderers = default;
        [SerializeField] RendererMaterialId[] _team1Renderers = default;

        public void SetColors(int teamId, Color color) {
            var renderers = teamId switch {
                0 => _team0Renderers,
                1 => _team1Renderers,
                _ => throw DebugUtility.NoMatch(teamId, nameof(teamId))
            };
            for (var i = 0; i < renderers.Length; i++) {
                ref var rm = ref renderers[i];
                
                if(!rm.Renderer) continue;
                
                var materials = rm.Renderer.sharedMaterials;
                var material = new Material(materials[rm.Index]);
                material.color = color;
                materials[rm.Index] = material;
                rm.Renderer.sharedMaterials = materials;
            }
        }
    }
}