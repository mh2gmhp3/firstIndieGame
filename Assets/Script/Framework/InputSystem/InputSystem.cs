using CameraModule;
using GameSystem;
using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputModule
{
    [GameSystem(GameSystemPriority.INPUT_SYSTEM)]
    public partial class InputSystem : BaseGameSystem<InputSystem>
    {
        private IInputProcessor _inputProcessor;

        protected override bool DoInit()
        {
            _inputProcessor = new DefaultInputProcessor();
            return true;
        }

        protected override void DoUpdate()
        {
            _inputProcessor.DetectInput();
        }

        #region Public Static Method SetInputProcessor

        public static void SetInputProcessor(IInputProcessor inputProcessor)
        {
            _instance.DoSetInputProcessor(inputProcessor);
        }

        private void DoSetInputProcessor(IInputProcessor inputProcessor)
        {
            if (inputProcessor == null)
            {
                Log.LogError($"SetInputProcessor, inputProcessor is null");
                return;
            }

            _inputProcessor = inputProcessor;
        }

        #endregion

        #region Public Static Method SetInputSetting

        public static void SetInputSetting(InputSetting inputSetting)
        {
            _instance.DoSetInputSetting(inputSetting);
        }

        private void DoSetInputSetting(InputSetting inputSetting)
        {
            _inputProcessor.SetInputSetting(inputSetting);
        }

        #endregion

        #region Public Static Method RegisterInputReceiver

        public static void RegisterInputReceiver(IInputReceiver inputReceiver)
        {
            _instance.DoRegisterInputReceiver(inputReceiver);
        }

        private void DoRegisterInputReceiver(IInputReceiver inputReceiver)
        {
            _inputProcessor.RegisterInputReceiver(inputReceiver);
        }

        #endregion

        #region Public Static Method UnRegisterInputReceiver

        public static void UnRegisterInputReceiver(IInputReceiver inputReceiver)
        {
            _instance.DoUnRegisterInputReceiver(inputReceiver);
        }

        private void DoUnRegisterInputReceiver(IInputReceiver inputReceiver)
        {
            _inputProcessor.UnRegisterInputReceiver(inputReceiver);
        }

        #endregion
    }
}

