using AssetsModule;
using CameraModule;
using GameSystem;
using InputModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace GameMainSystem
{
    [GameSystem(GameSystemPriority.GAME_MAIN_SYSTEM)]
    public partial class GameMainSystem : BaseGameSystem<GameMainSystem>
    {

        protected override void DoEnterGameFlowEnterStep(int flowStep)
        {
            if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_GameCamera)
            {
                CameraSystem.SetCameraBehavior(new GameCameraBehavior());
            }
            else if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_GameController)
            {
                InitInput();
            }
            else if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_GameTest)
            {
                InitGameTest();
            }
        }
    }
}
