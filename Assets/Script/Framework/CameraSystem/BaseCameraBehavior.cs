using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CameraModule
{
    [Flags]
    public enum BaseCameraState
    {
        None = 0000_0000_0000,

        //0000_0000_0001~0000_0000_1000 遊戲主要攝影機模式
        ThirdPersonMode = 0000_0000_0001,

        //攝影機特殊行為
        FollowTarget = 0000_0001_0000,
    }

    public abstract partial class BaseCameraBehavior : ICameraBehavior
    {
        protected GameObject _cameraGo;
        protected Transform _cameraTrans;
        protected Camera _camera;

        protected Dictionary<int, CameraCommandMethod> _commandIdToMethod;

        protected BaseCameraState _state = BaseCameraState.None;

        public BaseCameraBehavior()
        {
            InitCommandMethod();

            _followTargetProcessor = new FollowTargetProcessor(this);
            _thirdPersonModeProcessor = new ThirdPersonModeProcessor(this);
        }

        private void InitCommandMethod()
        {
            _commandIdToMethod = new Dictionary<int, CameraCommandMethod>();
            Type type = GetType();
            MethodInfo[] methodInfos =
                type.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);
            foreach (var methodInfo in methodInfos)
            {
                var attr = methodInfo.GetCustomAttribute<CameraCommandAttribute>();
                if (attr == null)
                    continue;

                if (_commandIdToMethod.ContainsKey(attr.CommandId))
                {
                    Log.LogWarning(
                        $"{GetType().Name} InitCommandMethod, " +
                        $"duplicate commandId Id:{attr.CommandId}");
                    continue;
                }

                CameraCommandMethod delg =
                    methodInfo.CreateDelegate(typeof(CameraCommandMethod), this) as CameraCommandMethod;
                if (delg == null)
                {
                    Log.LogWarning(
                        $"{GetType().Name} InitCommandMethod, " +
                        $"CreateDelegate failed commandId Id:{attr.CommandId}");
                    continue;
                }

                _commandIdToMethod.Add(attr.CommandId, delg);
            }
        }

        protected void RaiseStateFlag(BaseCameraState state)
        {
            _state |= state;
        }

        protected void FallStateFlag(BaseCameraState state)
        {
            _state ^= state;
        }

        public virtual void SetCamera(
            GameObject cameraGo,
            Transform cameraTrans,
            Camera camera,
            Transform cameraSystemTrans)
        {
            _cameraGo = cameraGo;
            _cameraTrans = cameraTrans;
            _camera = camera;
        }

        public virtual void CameraCommand(ICameraCommand command)
        {
            if (command == null)
                return;

            if (!_commandIdToMethod.TryGetValue(command.CommandId, out var method))
            {
                Log.LogWarning($"{GetType().Name} CameraCommand, " +
                    $"Command not found Id:{command.CommandId}");
                return;
            }

            method.Invoke(command);
        }

        public virtual void DoUpdate()
        {
            //TODO 這部分應該可以改成各自Flag呼叫對應的Processor
            if (_state.HasFlag(BaseCameraState.FollowTarget))
            {
                _followTargetProcessor.Update();
            }
            else if (_state.HasFlag(BaseCameraState.ThirdPersonMode))
            {
                _thirdPersonModeProcessor.Update();
            }
        }

        public virtual void DoFixedUpdate()
        {

        }

        public virtual void DoLateUpdate()
        {

        }
    }
}
