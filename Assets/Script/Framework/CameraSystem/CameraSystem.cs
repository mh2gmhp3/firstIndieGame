using GameSystem;
using AssetsModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Logging;

namespace CameraModule
{
    [GameSystem(GameSystemPriority.CAMERA_SYSTEM)]
    public partial class CameraSystem : BaseGameSystem<CameraSystem>
    {
        private const string CAMERA_RESOURCE_FRAMEWORK_PATH = "Framework/Camera/MainCamera";

        private GameObject _cameraGo = null;
        private Transform _cameraTrans = null;
        private Camera _cameraComp = null;

        private ICameraBehavior _cameraBehavior = null;

        protected override void DoEnterGameFlowEnterStep(int flowStep)
        {
            if (flowStep == (int)EnterGameFlowStepDefine.FrameworkEnterGameFlowStep.Init_BaseMainCamera)
            {
                var go = AssetsSystem.LoadAssets<GameObject>(CAMERA_RESOURCE_FRAMEWORK_PATH);
                _cameraGo = ObjectUtility.InstantiateWithoutClone(go);
                _cameraTrans = _cameraGo.transform;
                _cameraTrans.SetParent(_transform);
                _cameraComp = _cameraGo.GetComponent<Camera>();
                if (_cameraComp == null)
                {
                    Log.LogError($"DoEnterGameFlowEnterStep Init_BaseMainCamera, _cameraComp is null, Camera Component not found");
                    return;
                }
                SetCameraBehavior(new DefaultCameraBehavior());
            }
        }

        protected override void DoUpdate()
        {
            _cameraBehavior.DoUpdate();
        }

        protected override void DoFixedUpdate()
        {
            _cameraBehavior.DoFixedUpdate();
        }

        protected override void DoLateUpdate()
        {
            _cameraBehavior.DoLateUpdate();
        }

        #region Public Static Method SetCameraBehavior

        public static void SetCameraBehavior(ICameraBehavior cameraBehavior)
        {
            _instance.DoSetCameraBehavior(cameraBehavior);
        }

        private void DoSetCameraBehavior(ICameraBehavior cameraBehavior)
        {
            if (cameraBehavior == null)
            {
                Log.LogError($"SetCameraBehavior, cameraBehavior is null");
                return;
            }

            cameraBehavior.SetCamera(
                _cameraGo,
                _cameraTrans,
                _cameraComp,
                _transform);

            _cameraBehavior = cameraBehavior;
        }

        #endregion
    }
}
