using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraModule
{
    [Flags]
    public enum CameraState
    {
        None = 0000_0000_0000,

        //0000_0000_0001~0000_0000_1000 遊戲主要攝影機模式
        ThirdPersonMode = 0000_0000_0001,

        //攝影機特殊行為
        FollowTarget = 0000_0001_0000,
    }

    public class GameCameraBehavior : BaseCameraBehavior
    {
        public GameCameraBehavior() : base()
        {
            _followTargetProcessor = new FollowTargetProcessor(this);
            _thirdPersonModeProcessor = new ThirdPersonModeProcessor(this);
        }

        #region ThirdPersonMode

        protected ThirdPersonModeProcessor _thirdPersonModeProcessor;

        [CameraCommand((int)CameraCommandDefine.BaseCommand.SetThirdPersonMode)]
        private void SetThirdPersonMode(ICameraCommand command)
        {
            if (!(command is IThirdPersonModeCommandData commandData))
                return;

            _thirdPersonModeProcessor.InitThirdPersonMode(commandData);
            RaiseStateFlag((long)CameraState.ThirdPersonMode);
        }

        [CameraCommand((int)CameraCommandDefine.BaseCommand.UpdateThirdPersonScreenAxisValue)]
        private void UpdateThirdPersonScreenAxisValue(ICameraCommand command)
        {
            if (!(command is IUpdateThirdPersonScreenAxisData commandData))
                return;

            _thirdPersonModeProcessor.UpdateScreenAxis(commandData);
        }

        [CameraCommand((int)CameraCommandDefine.BaseCommand.UpdateThirdPersonSetting)]
        private void UpdateThirdPersonSetting(ICameraCommand command)
        {
            if (!(command is UpdateThirdPersonSettingData commandData))
                return;

            _thirdPersonModeProcessor.UpdateSetting(commandData);
        }

        #endregion

        #region FollowTarget

        protected FollowTargetProcessor _followTargetProcessor;

        [CameraCommand((int)CameraCommandDefine.BaseCommand.FollowTarget)]
        protected virtual void FollowTarget(ICameraCommand command)
        {
            if (!(command is IFollowTarget followTarget))
                return;

            _followTargetProcessor.SetFollowTargetAndDistance(
                followTarget.TargetTrans,
                followTarget.Distance);

            RaiseStateFlag((long)CameraState.FollowTarget);
        }

        #endregion

        #region ICameraBehavior

        public override void DoUpdate()
        {

        }

        public override void DoFixedUpdate()
        {
            if (StateHasFlag((long)CameraState.ThirdPersonMode))
            {
                _thirdPersonModeProcessor.FixedUpdate();
            }
        }

        public override void DoLateUpdate()
        {
            //TODO 這部分應該可以改成各自Flag呼叫對應的Processor
            if (StateHasFlag((long)CameraState.FollowTarget))
            {
                _followTargetProcessor.LateUpdate();
            }
        }

        public override void DoDrawGizmos()
        {
            _thirdPersonModeProcessor.DoDrawGizmos();
        }

        #endregion
    }
}
