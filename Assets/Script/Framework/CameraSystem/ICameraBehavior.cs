using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CameraModule
{
    public interface ICameraBehavior
    {
        public void SetCamera(
            GameObject cameraGo,
            Transform cameraTrans,
            Camera camera,
            Transform cameraSystemTrans);

        public void CameraCommand(ICameraCommand command);

        public void DoUpdate();
        public void DoFixedUpdate();
        public void DoLateUpdate();
    }

    public abstract class BaseCameraBehavior : ICameraBehavior
    {
        protected GameObject _cameraGo;
        protected Transform _cameraTrans;
        protected Camera _camera;

        protected Dictionary<int, CameraCommandMethod> _commandIdToMethod;

        public BaseCameraBehavior()
        {
            InitCommandMethod();
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

        }

        public virtual void DoFixedUpdate()
        {

        }

        public virtual void DoLateUpdate()
        {

        }

        [CameraCommand((int)CameraCommandDefine.BaseCommand.LookAtPosition)]
        protected virtual void LookAtPosition(ICameraCommand command)
        {

        }
    }

    public class DefaultCameraBehavior : BaseCameraBehavior
    {

    }
}
