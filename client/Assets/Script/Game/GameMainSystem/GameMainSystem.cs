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
                InitGameMain();
            }
        }

        protected override bool DoEnterGameFlowProcessStep(int flowStep)
        {
            return base.DoEnterGameFlowProcessStep(flowStep);
        }

        protected override void DoUpdate()
        {
            _characterController.DoUpdate();
            _collisionAreaManager.DoUpdate();

            if (TestMode)
            {
                RepeatCreateTestCollisionArea();
            }
        }

        protected override void DoFixedUpdate()
        {
            _characterController.DoFixedUpdate();
        }

        private void OnGUI()
        {
            _characterController.DoOnGUI();
            _collisionAreaManager.DoOnGUI();
        }

        private void OnDrawGizmos()
        {
            _collisionAreaManager.DoDrawGizmos();
        }
    }
}
