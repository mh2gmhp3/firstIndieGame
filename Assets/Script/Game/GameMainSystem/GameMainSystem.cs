using AssetsModule;
using CameraModule;
using GameSystem;
using InputModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainSystem
{
    [GameSystem(GameSystemPriority.GAME_MAIN_SYSTEM)]
    public class GameMainSystem : BaseGameSystem<GameMainSystem>
    {
        private const string INPUT_SETTING = "Setting/InputSetting";
        public GameInputReceiver _inputReceiver = null;

        protected override void DoEnterGameFlowEnterStep(int flowStep)
        {
            if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_GameCamera)
            {
                CameraSystem.SetCameraBehavior(new CameraBehavior());
            }
            else if (flowStep == (int)EnterGameFlowStepDefine.EnterGameFlowStep.Init_GameController)
            {
                var inputSetting = AssetsSystem.LoadAssets<InputSetting>(INPUT_SETTING);
                _inputReceiver = new GameInputReceiver(OnKeyDown, OnKeyUp);
                InputSystem.SetInputProcessor(new GameInputProcessor());
                InputSystem.SetInputSetting(inputSetting);
                InputSystem.RegisterInputReceiver(_inputReceiver);
            }
        }

        private void OnKeyDown(KeyCode keyCode, string command)
        {
            Log.LogInfo($"OnKeyDown KeyCode:{keyCode}, Command:{command}");
        }

        private void OnKeyUp(KeyCode keyCode, string command)
        {
            Log.LogInfo($"OnKeyUp KeyCode:{keyCode}, Command:{command}");
        }
    }
}
