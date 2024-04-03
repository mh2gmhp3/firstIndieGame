using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraModule
{
    public class CameraBehavior : ICameraBehavior
    {
        private Transform _cameraTrans = null;

        void ICameraBehavior.SetCamera(
            GameObject cameraGo,
            Transform cameraTrans,
            Camera camera,
            Transform cameraSystemTrans)
        {
            _cameraTrans = cameraTrans;
        }

        void ICameraBehavior.DoUpdate()
        {

        }

        void ICameraBehavior.DoFixedUpdate()
        {

        }

        void ICameraBehavior.DoLateUpdate()
        {

        }
    }
}
