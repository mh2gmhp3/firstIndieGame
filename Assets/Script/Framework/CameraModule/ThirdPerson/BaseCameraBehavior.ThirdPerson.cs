using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CameraModule
{
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
        /// 螢幕XY軸操作值
        /// </summary>
        public Vector2 ScreenAxisValue { get; set; }
    }

    public class ThirdPersonModeCommandData : IThirdPersonModeCommandData
    {
        public int CommandId { get; set; }
        public Transform TargetTrans { get; set; }
        public Vector3 FocusTargetOffset { get; set; }
        public float Distance { get; set; }
        public float CameraRotateSensitivity { get; set; }
        public Vector2 ScreenAxisValue { get; set; }

        public ThirdPersonModeCommandData()
        {
            CommandId = (int)CameraCommandDefine.BaseCommand.SetThirdPersonMode;
        }
    }

    #endregion

    public partial class BaseCameraBehavior
    {
        protected class ThirdPersonModeProcessor
        {
            protected BaseCameraBehavior _baseCameraBehavior;

            protected Transform _targetTrans;
            protected Vector3 _focusTargetOffset;

            protected float _distance;
            protected float _cameraRotateSensitivity = 10;
            protected Vector2 _screenAxis;

            protected Vector2 _cameraRotateValue = Vector2.zero;

            protected bool _active;

            private Vector3 _lookAtPosition = Vector3.zero;

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
                _screenAxis = commandData.ScreenAxisValue;

                _cameraRotateSensitivity = commandData.CameraRotateSensitivity;

                _active = true;
            }

            public void UpdateScreenAxis(IUpdateThirdPersonScreenAxisData commandData)
            {
                _screenAxis = commandData.ScreenAxis;
            }

            public void LateUpdate()
            {
                if (!_active)
                    return;
                if (_targetTrans == null)
                    return;

                _cameraRotateValue.x += _screenAxis.x * _cameraRotateSensitivity * Time.deltaTime;
                _cameraRotateValue.y -= _screenAxis.y * _cameraRotateSensitivity * Time.deltaTime;

                _cameraRotateValue.x = _cameraRotateValue.x < 0 ?
                    _cameraRotateValue.x + 360 :
                    _cameraRotateValue.x;
                _cameraRotateValue.x = _cameraRotateValue.x > 360 ?
                    _cameraRotateValue.x - 360 :
                    _cameraRotateValue.x;

                var rotationEuler = Quaternion.Euler(_cameraRotateValue.y, _cameraRotateValue.x, 0);
                _lookAtPosition = _targetTrans.position +
                    Quaternion.Euler(_baseCameraBehavior._cameraTrans.forward) * _focusTargetOffset;
                var cameraPosition = rotationEuler * new Vector3(0, 0, -_distance) + _lookAtPosition;

                bool needNotify = false;
                needNotify |= _baseCameraBehavior._cameraTrans.rotation != rotationEuler;
                needNotify |= _baseCameraBehavior._cameraTrans.position != cameraPosition;

                _baseCameraBehavior._cameraTrans.rotation = rotationEuler;
                _baseCameraBehavior._cameraTrans.position = cameraPosition;

                if (needNotify)
                {
                    _baseCameraBehavior.CameraNotify(
                        CameraNotifyReason.ThirdPersonModify,
                        _baseCameraBehavior._cameraTrans.position,
                        _baseCameraBehavior._cameraTrans.rotation);
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

        protected ThirdPersonModeProcessor _thirdPersonModeProcessor;

        [CameraCommand((int)CameraCommandDefine.BaseCommand.SetThirdPersonMode)]
        protected virtual void SetThirdPersonMode(ICameraCommand command)
        {
            if (!(command is IThirdPersonModeCommandData commandData))
                return;

            _thirdPersonModeProcessor.InitThirdPersonMode(commandData);
            RaiseStateFlag(BaseCameraState.ThirdPersonMode);
        }

        [CameraCommand((int)CameraCommandDefine.BaseCommand.UpdateThirdPersonScreenAxisValue)]
        protected virtual void UpdateThirdPersonScreenAxisValue(ICameraCommand command)
        {
            if (!(command is IUpdateThirdPersonScreenAxisData commandData))
                return;

            _thirdPersonModeProcessor.UpdateScreenAxis(commandData);
        }
    }
}
