using UnityEngine;

namespace Analog.Core
{
    // Legacy camera script kept only to avoid missing-script warnings on existing scenes.
    // SimpleCameraFollow now handles all camera movement.
    public class ThirdPersonCamera : MonoBehaviour
    {
        private void Awake()
        {
            enabled = false;
        }

        public void SetTarget(Transform newTarget) { }
    }
}
