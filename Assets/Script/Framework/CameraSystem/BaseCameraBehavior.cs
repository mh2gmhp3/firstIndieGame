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
        FollowTarget = 0000_0000_0001,
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
            if (_state.HasFlag(BaseCameraState.FollowTarget))
            {
                _followTargetProcessor.Update();
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
