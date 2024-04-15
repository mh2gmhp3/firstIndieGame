using CameraModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainSystem
{
    public partial class GameMainSystem : ICameraNotifyTarget
    {
        private void InitCamera()
        {
            CameraSystem.SetCameraBehavior(new GameCameraBehavior());
            CameraSystem.CameraCommand(new RegisterCameraNotifyTarget(this));
        }

        public void OnCameraNotify(int notifyReason, Vector3 position, Quaternion rotation)
        {
            if (notifyReason == (int)BaseCameraBehavior.CameraNotifyReason.ThirdPersonModify)
            {
                _characterController.SetMoveQuaternion(rotation);
            }
        }
    }
}
