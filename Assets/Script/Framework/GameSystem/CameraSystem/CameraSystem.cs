using GameSystem.Framework.Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace GameSystem.Framework.Camera
{
    [GameSystem(GameSystemPriority.CAMERA_SYSTEM)]
    public partial class CameraSystem : BaseGameSystem<CameraSystem>
    {
        private const string CAMERA_RESOURCE_FRAMEWORK_PATH = "Framework/Camera/MainCamera";

        private GameObject _cameraGo = null;
        private Transform _cametaTrans = null;

        protected override void DoEnterGameFlowEnterStep(int flowStep)
        {
            if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_BaseMainCamera)
            {
                var go = AssetsSystem.LoadAssets<GameObject>(CAMERA_RESOURCE_FRAMEWORK_PATH);
                _cameraGo = ObjectUtility.InstantiateWithoutClone(go);
                _cametaTrans = _cameraGo.transform;
                _cametaTrans.SetParent(_transform);
            }
        }
    }
}
