using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static CameraModule.BaseCameraBehavior;

namespace CameraModule
{
    #region ThirdPersonImmediateLooAtTarget

    public class ThirdPersonImmediateLooAtTarget : ICameraCommand
    {
        public int CommandId { get; set; }
        public Vector3 Direction;

        public ThirdPersonImmediateLooAtTarget(Vector3 direction)
        {
            CommandId = (int)CameraCommandDefine.BaseCommand.ThirdPersonImmediateLooAtTarget;
            Direction = direction;
        }
    }

    #endregion

    #region UpdateThirdPersonSettingData

    public class UpdateThirdPersonSettingData : ICameraCommand
    {
        public int CommandId { get; set; }

        public Vector3 FocusTargetOffset;
        public float Distance;
        public float CameraRotateSensitivity;
        public float CameraSpeed;

        public UpdateThirdPersonSettingData()
        {
            CommandId = (int)CameraCommandDefine.BaseCommand.UpdateThirdPersonSetting;
        }
    }

    #endregion

    #region IUpdateThirdPersonScreenAxisData

    public interface IUpdateThirdPersonScreenAxisData : ICameraCommand
    {
        /// <summary>
        /// 螢幕XY軸操作值
        /// </summary>
        public Vector2 ScreenAxis { get; set; }
    }

    public class UpdateThirdPersonScreenAxisData : IUpdateThirdPersonScreenAxisData
    {
        public int CommandId { get; set; }
        public Vector2 ScreenAxis { get; set; }

        public UpdateThirdPersonScreenAxisData()
        {
            CommandId = (int)CameraCommandDefine.BaseCommand.UpdateThirdPersonScreenAxisValue;
        }
    }

    #endregion

    #region IThirdPersonModeCommandData

    public interface IThirdPersonModeCommandData : ICameraCommand
    {
        /// <summary>
        /// 第三人稱使用的對象
        /// </summary>
        public Transform TargetTrans { get; set; }
        /// <summary>
        /// 螢幕XY軸操作值
        /// </summary>
        public Vector2 ScreenAxisValue { get; set; }
        /// <summary>
        /// 攝影機關注對象時使用的調整值
        /// </summary>
        public Vector3 FocusTargetOffset { get; set; }
        /// <summary>
        /// 設定時攝影機與對象距離
        /// </summary>
        public float Distance { get; set; }
        /// <summary>
        /// 攝影機旋轉靈敏度
        /// </summary>
        public float CameraRotateSensitivity { get; set; }
        /// <summary>
        /// 攝影機旋轉移動速度
        /// </summary>
        public float CameraSpeed { get; set; }
    }

    public class ThirdPersonModeCommandData : IThirdPersonModeCommandData
    {
        public int CommandId { get; set; }
        public Transform TargetTrans { get; set; }
        public Vector2 ScreenAxisValue { get; set; }
        public Vector3 FocusTargetOffset { get; set; }
        public float Distance { get; set; }
        public float CameraRotateSensitivity { get; set; }
        public float CameraSpeed { get; set; }

        public ThirdPersonModeCommandData()
        {
            CommandId = (int)CameraCommandDefine.BaseCommand.SetThirdPersonMode;
        }

        public ThirdPersonModeCommandData(Transform targetTrans, Vector2 screenAxisValue, UpdateThirdPersonSettingData settingData)
        {
            CommandId = (int)CameraCommandDefine.BaseCommand.SetThirdPersonMode;

            TargetTrans = targetTrans;
            ScreenAxisValue = screenAxisValue;
            FocusTargetOffset = settingData.FocusTargetOffset;
            Distance = settingData.Distance;
            CameraRotateSensitivity = settingData.CameraRotateSensitivity;
            CameraSpeed = settingData.CameraSpeed;
        }
    }

    #endregion

    public class ThirdPersonModeProcessor
    {
        private BaseCameraBehavior _baseCameraBehavior;

        private Transform _targetTrans;
        private Vector3 _focusTargetOffset;

        private float _distance;
        private float _cameraRotateSensitivity = 10;
        private Vector2 _screenInputAxis;

        private Vector2 _cameraRotateValue = Vector2.zero;

        private float _camMoveSpeed = 10f;

        private bool _active;

        private Vector3 _lookAtPosition = Vector3.zero;

        private bool _triggerThirdPersonImmediateLooAtTarget = false;
        private Vector3 _thirdPersonImmediateLooAtTargetDirection;

        public ThirdPersonModeProcessor(BaseCameraBehavior baseCameraBehavior)
        {
            _baseCameraBehavior = baseCameraBehavior;
            _active = false;
        }

        public void InitThirdPersonMode(IThirdPersonModeCommandData commandData)
        {
            _targetTrans = commandData.TargetTrans;
            _focusTargetOffset = commandData.FocusTargetOffset;

            _distance = commandData.Distance;
            _cameraRotateSensitivity = commandData.CameraRotateSensitivity;
            _screenInputAxis = commandData.ScreenAxisValue;

            _camMoveSpeed = commandData.CameraSpeed;

            _active = true;
        }

        public void UpdateScreenAxis(IUpdateThirdPersonScreenAxisData commandData)
        {
            _screenInputAxis = commandData.ScreenAxis;
        }

        public void UpdateSetting(UpdateThirdPersonSettingData commandData)
        {
            _focusTargetOffset = commandData.FocusTargetOffset;
            _distance = commandData.Distance;
            _cameraRotateSensitivity = commandData.CameraRotateSensitivity;
        }

        public void ThirdPersonImmediateLooAtTarget(ThirdPersonImmediateLooAtTarget commandData)
        {
            _triggerThirdPersonImmediateLooAtTarget = true;
            _thirdPersonImmediateLooAtTargetDirection = commandData.Direction;
        }

        public void FixedUpdate()
        {
            if (!_active)
                return;
            if (_targetTrans == null)
                return;

            if (_triggerThirdPersonImmediateLooAtTarget)
            {
                var eulerAngles = Quaternion.LookRotation(_thirdPersonImmediateLooAtTargetDirection).eulerAngles;
                _cameraRotateValue = new Vector2(eulerAngles.y, eulerAngles.x);
            }
            else
            {
                _cameraRotateValue.x += _screenInputAxis.x * _cameraRotateSensitivity * Time.deltaTime;
                _cameraRotateValue.y -= _screenInputAxis.y * _cameraRotateSensitivity * Time.deltaTime;
            }

            _cameraRotateValue.x = _cameraRotateValue.x < 0 ?
                _cameraRotateValue.x + 360 :
                _cameraRotateValue.x;
            _cameraRotateValue.x = _cameraRotateValue.x > 360 ?
                _cameraRotateValue.x - 360 :
                _cameraRotateValue.x;

            var rotationEuler = Quaternion.Euler(_cameraRotateValue.y, _cameraRotateValue.x, 0);
            _lookAtPosition = _targetTrans.position +
                _baseCameraBehavior.CameraTrans.rotation * _focusTargetOffset;

            float maxDistance = _distance;
            if (Physics.Raycast(_lookAtPosition, (_baseCameraBehavior.CameraTrans.position - _lookAtPosition).normalized, out RaycastHit hitInfo))
            {
                maxDistance = Mathf.Clamp(maxDistance, 0, hitInfo.distance);
            }

            var cameraPosition = rotationEuler * new Vector3(0, 0, -maxDistance) + _lookAtPosition;

            bool needNotify = false;
            needNotify |= _baseCameraBehavior.CameraTrans.rotation != rotationEuler;
            needNotify |= _baseCameraBehavior.CameraTrans.position != cameraPosition;

            _baseCameraBehavior.CameraTrans.rotation = rotationEuler;
            if (_triggerThirdPersonImmediateLooAtTarget)
            {
                _baseCameraBehavior.CameraTrans.position = cameraPosition;
            }
            else
            {
                _baseCameraBehavior.CameraTrans.position =
                    Vector3.MoveTowards(
                        _baseCameraBehavior.CameraTrans.position,
                        cameraPosition,
                        _camMoveSpeed * Time.deltaTime);
            }

            if (_triggerThirdPersonImmediateLooAtTarget)
            {
                _triggerThirdPersonImmediateLooAtTarget = false;
            }

            if (needNotify)
            {
                _baseCameraBehavior.CameraNotify(
                    CameraNotifyReason.ThirdPersonModify,
                    _baseCameraBehavior.CameraTrans.position,
                    _baseCameraBehavior.CameraTrans.rotation);
            }
        }

        public void DoDrawGizmos()
        {
#if UNITY_EDITOR
            //Color color = Gizmos.color;
            //Gizmos.color = Color.red;
            //Gizmos.DrawSphere(_lookAtPosition, 0.5f);
            //Gizmos.color = color;
#endif
        }
    }
}
