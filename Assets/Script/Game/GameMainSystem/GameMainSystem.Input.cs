using AssetsModule;
using InputModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainSystem
{
    public partial class GameMainSystem
    {
        private const string INPUT_SETTING = "Setting/InputSetting";
        public GameInputReceiver _inputReceiver = null;

        private void InitInput()
        {
            var inputSetting = AssetsSystem.LoadAssets<InputSetting>(INPUT_SETTING);
            _inputReceiver =
                new GameInputReceiver(
                    OnKeyDown,
                    OnKeyUp,
                    OnKeyHold);
            InputSystem.SetInputProcessor(new GameInputProcessor());
            //InputSystem.SetInputProcessor(new AllInpuPrcocessor());
            InputSystem.SetInputSetting(inputSetting);
            InputSystem.RegisterInputReceiver(_inputReceiver);
        }

        private void OnKeyDown(KeyCode keyCode, string command)
        {
            Log.LogInfo($"OnKeyDown KeyCode:{keyCode}, Command:{command}");
        }

        private void OnKeyUp(KeyCode keyCode, string command)
        {
            Log.LogInfo($"OnKeyUp KeyCode:{keyCode}, Command:{command}");
        }

        private void OnKeyHold(KeyCode keyCode, string command)
        {
            Log.LogInfo($"OnKeyHold KeyCode:{keyCode}, Command:{command}");
        }
    }
}
