using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

        public void CameraCommand(ICameraCommand command);

        public void DoUpdate();
        public void DoFixedUpdate();
        public void DoLateUpdate();
        public void DoDrawGizmos();
    }
}
