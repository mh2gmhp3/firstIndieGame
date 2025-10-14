using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputModule
{
    public class AllInputProcesser : BaseInputProcessor<IInputReceiver>
    {
        public override void SetInputSetting(InputSetting inputSetting)
        {
            _observerKeyCodeList.Clear();
            _keyCodeToTriggerCommandListDic.Clear();
            var allKeys = Enum.GetValues(typeof(KeyCode));
            foreach (KeyCode keyCode in allKeys)
            {
                //Unity對應多平台KeyCode內有相同的編號
                if (_keyCodeToTriggerCommandListDic.ContainsKey(keyCode))
                    continue;

                var keyCodeTriggerCommand = new RuntimeKeyCodeTriggerCommand();

                string keyCodeString = keyCode.ToString();
                keyCodeTriggerCommand.KeyDownCommandList.Add(keyCodeString);
                keyCodeTriggerCommand.KeyUpCommandList.Add(keyCodeString);
                keyCodeTriggerCommand.KeyHoldCommandList.Add(keyCodeString);

                _observerKeyCodeList.Add(keyCode);
                _keyCodeToTriggerCommandListDic.Add(keyCode, keyCodeTriggerCommand);
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

        protected override void OnKeyHold(KeyCode keyCode, string command)
        {
            Log.LogInfo($"OnKeyHold : {keyCode}");
        }
    }
}
