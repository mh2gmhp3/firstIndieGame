using AssetsModule;
using CameraModule;
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

            _inputReceiver.RegisterAxisValueChangedEvent(
                new List<string>
                {
                    "Mouse X",
                    "Mouse Y"
                },
                OnScreenAxisChanged);
            _inputReceiver.RegisterAxisValueChangedEvent(
                new List<string>
                {
                    "Right Stick Horizontal",
                    "Right Stick Vertical"
                },
                OnScreenAxisChanged);
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

        private UpdateThirdPersonScreenAxisData _updateThirdPersonScreenAxisData =
            new UpdateThirdPersonScreenAxisData();
        private void OnScreenAxisChanged(List<float> values)
        {
            _updateThirdPersonScreenAxisData.ScreenAxis = new Vector2(values[0], values[1]);
            CameraSystem.CameraCommand(_updateThirdPersonScreenAxisData);
        }
    }
}
