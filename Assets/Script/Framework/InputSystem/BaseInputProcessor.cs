using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InputModule
{
    public class KeyboardRuntimeInputSetting
    {
        public KeyCode KeyCode;
        public string Command;

        public KeyboardRuntimeInputSetting(KeyboardInputSetting keyboardInputSetting)
        {
            KeyCode = keyboardInputSetting.KeyCode;
            Command = keyboardInputSetting.Command;
        }
    }

    public abstract class BaseInputProcessor<T> : IInputProcessor
        where T : IInputReceiver
    {
        protected List<KeyboardRuntimeInputSetting> _keyboardRuntimeSettings =
              new List<KeyboardRuntimeInputSetting>();

        protected HashSet<IInputReceiver> _inputReceiverSet = new HashSet<IInputReceiver>();
        protected List<T> _inputReceiverList = new List<T>();

        public virtual void DetectKeyboardInput()
        {
            int count = _keyboardRuntimeSettings.Count;
            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
            {
                var setting = _keyboardRuntimeSettings[i];
                if (Input.GetKeyDown(setting.KeyCode))
                {
                    OnKeyDown(setting.KeyCode, setting.Command);
                }
                if (Input.GetKeyUp(setting.KeyCode))
                {
                    OnKeyUp(setting.KeyCode, setting.Command);
                }
            }
        }

        public void SetInputSetting(InputSetting inputSetting)
        {
            if (inputSetting == null)
                return;

            _keyboardRuntimeSettings.Clear();
            for (int i = 0; i < inputSetting.KeyboardInputSettingList.Count; i++)
            {
                _keyboardRuntimeSettings.Add(
                    new KeyboardRuntimeInputSetting(inputSetting.KeyboardInputSettingList[i]));
            }
        }

        public void RegisterInputReceiver(IInputReceiver inputReceiver)
        {
            if (_inputReceiverSet.Contains(inputReceiver))
                return;

            _inputReceiverSet.Add(inputReceiver);
            if (inputReceiver is T type)
                _inputReceiverList.Add(type);
        }

        public void UnRegisterInputReceiver(IInputReceiver inputReceiver)
        {
            if (!_inputReceiverSet.Contains(inputReceiver))
                return;

            _inputReceiverSet.Remove(inputReceiver);
            if (inputReceiver is T type)
                _inputReceiverList.Add(type);
        }

        protected virtual void OnKeyDown(KeyCode keyCode, string command) { }
        protected virtual void OnKeyUp(KeyCode keyCode, string command) { }
    }
}
