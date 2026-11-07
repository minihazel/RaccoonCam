using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace hazelify.RaccoonCam.Scripts
{
    public class Helmetcam : MonoBehaviour
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
