using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moball {
    public class DisableCamera : MonoBehaviour
    {
        public GameObject _cameraObject;
        public void Disable() {
            var obj = _cameraObject;
            if (obj != null) {
                obj.SetActive(false);
            }
        }
    }
}