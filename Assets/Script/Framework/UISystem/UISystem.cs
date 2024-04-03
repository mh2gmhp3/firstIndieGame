using AssetsSystem;
using GameSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace UISystem
{
    [GameSystem(GameSystemPriority.UI_SYSTEM)]
    public partial class UISystem : BaseGameSystem<UISystem>
    {
        private const string GUI_ROOT_RESOURCE_PATH = "Framework/UI/GUIRoot";

        private GameObject _guiRoot = null;
        private Transform _guiRootTrans = null;
        private RectTransform _guiRootRectTrans = null;

        protected override void DoEnterGameFlowEnterStep(int flowStep)
        {
            if (flowStep == (int)EnterGameFlowStepDefine.FrameworkEnterGameFlowStep.Init_GUIRoot)
            {
                var obj = AssetsSystem.AssetsSystem.LoadAssets<GameObject>(GUI_ROOT_RESOURCE_PATH);
                _guiRoot = ObjectUtility.InstantiateWithoutClone(obj);
                _guiRootTrans = _guiRoot.transform;
                _guiRootRectTrans = _guiRootTrans as RectTransform;

                _guiRootRectTrans.SetParent(_transform);
            }
        }
    }
}
