using GameSystem;
using AssetsSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace CameraSystem
{
    [GameSystem(GameSystemPriority.CAMERA_SYSTEM)]
    public partial class CameraSystem : BaseGameSystem<CameraSystem>
    {
        private const string CAMERA_RESOURCE_FRAMEWORK_PATH = "Framework/Camera/MainCamera";

        private GameObject _cameraGo = null;
        private Transform _cametaTrans = null;

        protected override void DoEnterGameFlowEnterStep(int flowStep)
        {
            if (flowStep == (int)EnterGameFlowStepDefine.FrameworkEnterGameFlowStep.Init_BaseMainCamera)
            {
                var go = AssetsSystem.AssetsSystem.LoadAssets<GameObject>(CAMERA_RESOURCE_FRAMEWORK_PATH);
                _cameraGo = ObjectUtility.InstantiateWithoutClone(go);
                _cametaTrans = _cameraGo.transform;
                _cametaTrans.SetParent(_transform);
            }
        }
    }
}
