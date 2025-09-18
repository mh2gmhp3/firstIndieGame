using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraModule
{
    public interface ICameraNotifyTarget
    {
        public void OnCameraNotify(int notifyReason, Vector3 position, Quaternion rotation);
    }
}
