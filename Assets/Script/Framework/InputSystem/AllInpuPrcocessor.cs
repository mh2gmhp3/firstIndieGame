using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputModule
{
    public class AllInpuPrcocessor : BaseInputProcessor<IInputReceiver>
    {
        public override void DetectKeyboardInput()
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            if (x != 0 || y != 0)
                Log.LogInfo(new Vector2(x, y));

            base.DetectKeyboardInput();
        }

        public override void SetInputSetting(InputSetting inputSetting)
        {
            _keyboardRuntimeSettings.Clear();
            var allKeys = Enum.GetValues(typeof(KeyCode));
            foreach (KeyCode key in allKeys)
            {
                _keyboardRuntimeSettings.Add(
                    new KeyboardRuntimeInputSetting(
                        key,
                        key.ToString()));
            }
        }

        protected override void OnKeyDown(KeyCode keyCode, string command)
        {
            Log.LogInfo($"OnKeyDown : {keyCode}");
        }

        protected override void OnKeyUp(KeyCode keyCode, string command)
        {
            Log.LogInfo($"OnKeyUp : {keyCode}");
        }
    }
}
