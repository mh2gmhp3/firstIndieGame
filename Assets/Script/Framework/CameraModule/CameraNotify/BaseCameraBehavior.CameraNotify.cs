using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraModule
{
    #region IRegisterCameraNotifyTarget

    public interface IRegisterCameraNotifyTarget : ICameraCommand
    {
        /// <summary>
        /// 接收變動通知目標本身
        /// </summary>
        public ICameraNotifyTarget NotifyTarget { get; set; }
    }

    public class RegisterCameraNotifyTarget : IRegisterCameraNotifyTarget
    {
        public int CommandId { get; set; }
        public ICameraNotifyTarget NotifyTarget { get; set; }

        public RegisterCameraNotifyTarget(ICameraNotifyTarget notifyTarget)
        {
            CommandId = (int)CameraCommandDefine.BaseCommand.RegisterCameraNotifyTarget;
            NotifyTarget = notifyTarget;
        }
    }

    #endregion

    #region IUnRegisterCameraNotifyTarget

    public interface IUnRegisterCameraNotifyTarget : ICameraCommand
    {
        /// <summary>
        /// 接收變動通知目標本身
        /// </summary>
        public ICameraNotifyTarget NotifyTarget { get; set; }
    }

    public class UnRegisterCameraNotifyTarget : IUnRegisterCameraNotifyTarget
    {
        public int CommandId { get; set; }
        public ICameraNotifyTarget NotifyTarget { get; set; }

        public UnRegisterCameraNotifyTarget(ICameraNotifyTarget notifyTarget)
        {
            CommandId = (int)CameraCommandDefine.BaseCommand.UnRegisterCameraNotifyTarget;
            NotifyTarget = notifyTarget;
        }
    }

    #endregion

    public partial class BaseCameraBehavior
    {
        public enum CameraNotifyReason
        {
            ThirdPersonModify = 1,
        }

        private List<ICameraNotifyTarget> _cameraNotifyTargetList =
            new List<ICameraNotifyTarget>();

        public void CameraNotify(CameraNotifyReason notifyReason, Vector3 position, Quaternion rotation)
        {
            CameraNotify((int)notifyReason, position, rotation);
        }

        protected virtual void CameraNotify(int notifyReason, Vector3 position, Quaternion rotation)
        {
            for (int i = 0; i < _cameraNotifyTargetList.Count; i++)
            {
                var notifyTarget = _cameraNotifyTargetList[i];
                if (notifyTarget == null)
                    continue;

                notifyTarget.OnCameraNotify(notifyReason, position, rotation);
            }
        }

        [CameraCommand((int)CameraCommandDefine.BaseCommand.RegisterCameraNotifyTarget)]
        public void RegisterCameraNotifyTarget(ICameraCommand command)
        {
            if (!(command is IRegisterCameraNotifyTarget notifyTarget))
                return;

            if (notifyTarget.NotifyTarget == null)
                return;

            if (_cameraNotifyTargetList.Contains(notifyTarget.NotifyTarget))
                return;

            _cameraNotifyTargetList.Add(notifyTarget.NotifyTarget);
        }

        [CameraCommand((int)CameraCommandDefine.BaseCommand.UnRegisterCameraNotifyTarget)]
        public void UnRegisterCameraNotifyTarget(ICameraCommand command)
        {
            if (!(command is IUnRegisterCameraNotifyTarget notifyTarget))
                return;

            if (notifyTarget.NotifyTarget == null)
                return;

            _cameraNotifyTargetList.Remove(notifyTarget.NotifyTarget);
        }
    }
}
