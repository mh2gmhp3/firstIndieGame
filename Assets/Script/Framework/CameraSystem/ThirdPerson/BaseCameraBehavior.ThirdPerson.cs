using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CameraModule
{
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
        /// 設定時攝影機觀看對象的方向
        /// </summary>
        public Vector3 Direction { get; set; }
    }

    public class ThirdPersonModeCommandData : IThirdPersonModeCommandData
    {
        public int CommandId { get; set; }
        public Transform TargetTrans { get; set; }
        public Vector3 FocusTargetOffset { get; set; }
        public float Distance { get; set; }
        public Vector3 Direction { get; set; }
    }

    public partial class BaseCameraBehavior
    {
        protected class ThirdPersonModeProcessor
        {
            protected BaseCameraBehavior _baseCameraBehavior;

            protected Transform _targetTrans;
            protected Vector3 _focusTargetOffset;

            protected float _distance;
            protected Vector3 _direction;

            protected bool _active;

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
                _direction = commandData.Direction;

                _active = true;
            }

            public void Update()
            {
                if (!_active)
                    return;
                if (_targetTrans == null)
                    return;


            }
        }

        protected ThirdPersonModeProcessor _thirdPersonModeProcessor;

        [CameraCommand((int)CameraCommandDefine.BaseCommand.ThirdPersonMode)]
        protected virtual void ThirdPersonMode(ICameraCommand command)
        {
            if (!(command is IThirdPersonModeCommandData commandData))
                return;

            _thirdPersonModeProcessor.InitThirdPersonMode(commandData);
            RaiseStateFlag(BaseCameraState.ThirdPersonMode);
        }
    }
}
