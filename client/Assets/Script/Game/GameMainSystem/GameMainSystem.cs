using AssetModule;
using CameraModule;
using FormModule;
using GameSystem;
using InputModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UIModule;
using UnityEngine;
using Utility;

namespace GameMainModule
{
    [GameSystem(GameSystemPriority.GAME_MAIN_SYSTEM)]
    public partial class GameMainSystem : BaseGameSystem<GameMainSystem>
    {
        private List<IUpdateTarget> _updateTargetList = new List<IUpdateTarget>();

        protected override void DoEnterGameFlowEnterStep(int flowStep)
        {
            if (flowStep == (int)EnterGameFlowStepDefine.FrameworkEnterGameFlowStep.Init_FormTableDone)
            {
                FormSystem.InitGameTableGroup();
            }
            else if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_GameCamera)
            {
                InitCamera();
            }
            else if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_GameController)
            {
                InitInput();
            }
            else if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_GameTest)
            {
                InitGameData();
                InitAssetSetting();
                InitUnitManager();
                InitTerrainManager();
                CreateTestData(10);
                InitGameTest();
            }
            else if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_GameScene)
            {
                InitGameScene();
            }
            else if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_GameCollision)
            {
                InitCollision();
            }
            else if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_GameMain)
            {
                InitGameData();
                InitAssetSetting();
                InitUnitManager();
                InitTerrainManager();
                InitGameMain();
            }
        }

        protected override bool DoEnterGameFlowProcessStep(int flowStep)
        {
            return base.DoEnterGameFlowProcessStep(flowStep);
        }

        protected override void DoUpdate()
        {
            for (int i = 0; i < _updateTargetList.Count; i++)
            {
                _updateTargetList[i].DoUpdate();
            }
        }

        protected override void DoFixedUpdate()
        {
            for (int i = 0; i < _updateTargetList.Count; i++)
            {
                _updateTargetList[i].DoFixedUpdate();
            }
        }

        protected override void DoLateUpdate()
        {
            for (int i = 0; i < _updateTargetList.Count; i++)
            {
                _updateTargetList[i].DoLateUpdate();
            }
        }

        private void OnGUI()
        {
            for (int i = 0; i < _updateTargetList.Count; i++)
            {
                _updateTargetList[i].DoOnGUI();
            }
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < _updateTargetList.Count; i++)
            {
                _updateTargetList[i].DoDrawGizmos();
            }
        }

        private void RegisterUpdateTarget(IUpdateTarget updateTarget)
        {
            if (updateTarget == null)
                return;

            _updateTargetList.Add(updateTarget);
        }

        private void UnRegisterUpdateTarget(IUpdateTarget updateTarget)
        {
            _updateTargetList.Remove(updateTarget);
        }
    }
}
