using CameraModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    public enum GameThirdPersonCameraSetting
    {
        /// <summary>
        /// 一般背後位
        /// </summary>
        Normal,
        /// <summary>
        /// 過肩是繳
        /// </summary>
        OTP
    }

    public partial class GameMainSystem : ICameraNotifyTarget
    {
        private UpdateThirdPersonSettingData _normalThirdPersonSettingData;
        private UpdateThirdPersonSettingData _OTPThirdPersonSettingData;

        private GameThirdPersonCameraSetting _thirdPersonCameraSetting = GameThirdPersonCameraSetting.Normal;

        public static GameThirdPersonCameraSetting ThirdPersonCameraSetting => _instance._thirdPersonCameraSetting;

        private void InitCamera()
        {
            CameraSystem.SetCameraBehavior(new GameCameraBehavior());
            CameraSystem.CameraCommand(new RegisterCameraNotifyTarget(this));
            _normalThirdPersonSettingData = new UpdateThirdPersonSettingData()
            {
                FocusTargetOffset = new Vector3(0f, 2f, 0),
                Distance = 5,
                CameraRotateSensitivity = 100,
                CameraSpeed = 20
            };
            _OTPThirdPersonSettingData = new UpdateThirdPersonSettingData()
            {
                FocusTargetOffset = new Vector3(1f, 1.5f, 0),
                Distance = 3,
                CameraRotateSensitivity = 200,
                CameraSpeed = 20
            };
        }

        public void OnCameraNotify(int notifyReason, Vector3 position, Quaternion rotation)
        {
            if (notifyReason == (int)BaseCameraBehavior.CameraNotifyReason.ThirdPersonModify)
            {
                _characterController.SetMoveQuaternion(rotation);
            }
        }

        public void TriggerChangeCameraSetting()
        {
            if (_thirdPersonCameraSetting == GameThirdPersonCameraSetting.Normal)
            {
                ChangeToCameraSetting(GameThirdPersonCameraSetting.OTP);
            }
            else if (_thirdPersonCameraSetting == GameThirdPersonCameraSetting.OTP)
            {
                ChangeToCameraSetting(GameThirdPersonCameraSetting.Normal);
            }
        }

        public static void ChangeToCameraSetting(GameThirdPersonCameraSetting setting)
        {
            _instance.InternalChangeToCameraSetting(setting);
        }

        private void InternalChangeToCameraSetting(GameThirdPersonCameraSetting setting)
        {
            if (_thirdPersonCameraSetting == setting)
                return;

            _thirdPersonCameraSetting = setting;
            if (_thirdPersonCameraSetting == GameThirdPersonCameraSetting.Normal)
            {
                CameraSystem.CameraCommand(_normalThirdPersonSettingData);
            }
            else if (_thirdPersonCameraSetting == GameThirdPersonCameraSetting.OTP)
            {
                CameraSystem.CameraCommand(_OTPThirdPersonSettingData);
            }
        }
    }
}
