using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraModule
{
    public partial class CameraCommandDefine
    {
        public enum BaseCommand
        {
            SetThirdPersonMode = 1,
            UpdateThirdPersonScreenAxisValue = 2,
            UpdateThirdPersonSetting = 3,

            LookAtPosition = 10,
            FollowTarget = 11,

            RegisterCameraNotifyTarget = 20,
            UnRegisterCameraNotifyTarget = 21,
        }
    }
}
