using JetBrains.Annotations;
using UnityEngine;

namespace hazelify.RaccoonCam.Scripts
{
    public class Bodycam : MonoBehaviour
    {
        public bool IsActive = false;

        [UsedImplicitly]
        public void Update()
        {
            if (!IsActive)
            {
                return;
            }
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            Destroy(this);
        }
    }
}
