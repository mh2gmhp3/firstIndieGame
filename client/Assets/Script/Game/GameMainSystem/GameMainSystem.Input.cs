using AssetModule;
using CameraModule;
using InputModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private const string INPUT_SETTING = "Setting/InputSetting";
        private GameInputReceiver _normalInputReceiver = null;
        private GameInputReceiver _uiInputReceiver = null;
        private List<GameInputReceiver> _inputReceiverList = new List<GameInputReceiver>();


        private void InitInput()
        {
            var inputSetting = AssetSystem.LoadAsset<InputSetting>(INPUT_SETTING);
            InitNormalInput();
            InitUIInput();
            InputSystem.SetInputProcessor(new GameInputProcessor());
            //InputSystem.SetInputProcessor(new AllInpuPrcocessor());
            InputSystem.SetInputSetting(inputSetting);
        }


        private void UnRegisterAllInputReceiver()
        {
            for (int i = 0; i < _inputReceiverList.Count; i++)
            {
                InputSystem.UnRegisterInputReceiver(_inputReceiverList[i]);
            }
        }


        #region NoramlInput

        private void InitNormalInput()
        {
            _normalInputReceiver =
                new GameInputReceiver(
                    OnKeyDown,
                    OnKeyUp,
                    OnKeyHold);
            _inputReceiverList.Add(_normalInputReceiver);

            //滑鼠
            _normalInputReceiver.RegisterAxisValueChangedEvent(
                new List<string>
                {
                    "Mouse X",
                    "Mouse Y"
                },
                OnScreenAxisChanged);
            //右搖桿
            _normalInputReceiver.RegisterAxisValueChangedEvent(
                new List<string>
                {
                    "Right Stick Horizontal",
                    "Right Stick Vertical"
                },
                OnScreenAxisChanged);
            //左搖桿或鍵盤WASD方向
            _normalInputReceiver.RegisterAxisValueChangedEvent(
                new List<string>
                {
                    "Horizontal",
                    "Vertical"
                },
                OnMovementAxisChanged);
        }

        private void ChangeToNormalInput()
        {
            UnRegisterAllInputReceiver();
            InputSystem.RegisterInputReceiver(_normalInputReceiver);
        }

        private void OnKeyDown(KeyCode keyCode, string command)
        {
            //Log.LogInfo($"OnKeyDown KeyCode:{keyCode}, Command:{command}");
            if (command == "jump")
            {
                _characterController.Jump();
            }
            else if (command == "mainattack")
            {
                _characterController.MainAttack();
            }
            else if (command == "subattack")
            {
                _characterController.SubAttack();
            }
        }

        private void OnKeyUp(KeyCode keyCode, string command)
        {
            Log.LogInfo($"OnKeyUp KeyCode:{keyCode}, Command:{command}");
        }

        private void OnKeyHold(KeyCode keyCode, string command)
        {
            if (command == "run")
            {
                _characterController.Run();
            }
        }

        private UpdateThirdPersonScreenAxisData _updateThirdPersonScreenAxisData =
            new UpdateThirdPersonScreenAxisData();
        private void OnScreenAxisChanged(List<float> values)
        {
            _updateThirdPersonScreenAxisData.ScreenAxis = new Vector2(values[0], values[1]);
            CameraSystem.CameraCommand(_updateThirdPersonScreenAxisData);
        }

        private void OnMovementAxisChanged(List<float> values)
        {
            _characterController.SetMoveAxis(new Vector3(values[0], 0, values[1]));
        }

        #endregion

        #region UIInput

        private void InitUIInput()
        {
            _uiInputReceiver = new GameInputReceiver(
                null,
                null,
                null);
            _inputReceiverList.Add(_uiInputReceiver);
        }

        private void ChangeToUIInput()
        {
            UnRegisterAllInputReceiver();
            InputSystem.RegisterInputReceiver(_uiInputReceiver);
        }

        #endregion
    }
}
