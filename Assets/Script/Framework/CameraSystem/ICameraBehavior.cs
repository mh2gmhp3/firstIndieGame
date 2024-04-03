using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraModule
{
    public interface ICameraBehavior
    {
        public void SetCamera(
            GameObject cameraGo,
            Transform cameraTrans,
            Camera camera,
            Transform cameraSystemTrans);

        public void DoUpdate();
        public void DoFixedUpdate();
        public void DoLateUpdate();
    }

    public class DefaultCameraBehavior : ICameraBehavior
    {
        void ICameraBehavior.SetCamera(
            GameObject cameraGo,
            Transform cameraTrans,
            Camera camera,
            Transform cameraSystemTrans)
        {
            //do nothing
        }

        void ICameraBehavior.DoUpdate()
        {
            //do nothing
        }

        void ICameraBehavior.DoFixedUpdate()
        {
            //do nothing
        }

        void ICameraBehavior.DoLateUpdate()
        {
            //do nothing
        }
    }
}
