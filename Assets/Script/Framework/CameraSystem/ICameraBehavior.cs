using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraSystem
{
    public interface ICameraBehavior
    {
        public void SetCamera(Camera camera, Transform cameraSystemTrans);
    }


}
