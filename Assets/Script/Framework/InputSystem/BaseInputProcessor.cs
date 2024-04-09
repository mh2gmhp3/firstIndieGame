using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UIElements;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace InputModule
{
    public abstract class BaseInputProcessor<T> : IInputProcessor
        where T : IInputReceiver
    {
        protected class RuntimeKeyCodeTriggerCommand
        {
            public List<string> KeyDownCommandList = new List<string>();
            public List<string> KeyUpCommandList = new List<string>();
            public List<string> KeyHoldCommandList = new List<string>();

            public List<string> GetCommandListByType(KeyCodeTriggerType type)
            {
                switch (type)
                {
                    case KeyCodeTriggerType.KeyDown:
                        return KeyDownCommandList;
                    case KeyCodeTriggerType.KeyUp:
                        return KeyUpCommandList;
                    case KeyCodeTriggerType.KeyHold:
                        return KeyHoldCommandList;
                }

                return null;
            }
        }

        protected List<KeyCode> _observerKeyCodeList = new List<KeyCode>();
        protected Dictionary<KeyCode, RuntimeKeyCodeTriggerCommand> _keyCodeToTriggerCommandListDic =
              new Dictionary<KeyCode, RuntimeKeyCodeTriggerCommand>();
        protected List<KeyCode> _holdKeyCodeList = new List<KeyCode>();

        protected List<string> _observerAxisList = new List<string>();
        protected Dictionary<string, float> _axisToValueDic =
              new Dictionary<string, float>();

        protected List<T> _inputReceiverList = new List<T>();

        public virtual void DetectInput()
        {
            DetectInputKeyCode();
            DetectInputAxis();
        }

        public virtual void SetInputSetting(InputSetting inputSetting)
        {
            if (inputSetting == null)
                return;

            //KeyCode
            _observerKeyCodeList.Clear();
            _keyCodeToTriggerCommandListDic.Clear();
            for (int i = 0; i < inputSetting.InputCommandList.Count; i++)
            {
                var inputCommand = inputSetting.InputCommandList[i];
                int assignKeyCodeCount = inputCommand.AssignKeyCodeTriggerSettingList.Count;
                if (assignKeyCodeCount == 0)
                    continue;

                for (int j = 0; j < assignKeyCodeCount; j++)
                {
                    var keyCodeTriggerSetting = inputCommand.AssignKeyCodeTriggerSettingList[j];
                    var keyCode = keyCodeTriggerSetting.KeyCode;
                    if (!_keyCodeToTriggerCommandListDic.TryGetValue(keyCode, out var keyCodeTriggerCommand))
                    {
                        keyCodeTriggerCommand = new RuntimeKeyCodeTriggerCommand();
                        _observerKeyCodeList.Add(keyCode);
                        _keyCodeToTriggerCommandListDic.Add(keyCode, keyCodeTriggerCommand);
                    }

                    var commadList = keyCodeTriggerCommand.
                        GetCommandListByType(keyCodeTriggerSetting.Type);

                    if (commadList.Contains(inputCommand.Command))
                        continue;

                    commadList.Add(inputCommand.Command);
                }
            }

            //Axis
            _observerAxisList.Clear();
            _axisToValueDic.Clear();
            for (int i = 0; i < inputSetting.InputAxisList.Count; i++)
            {
                var inputAxis = inputSetting.InputAxisList[i];
                if (_axisToValueDic.ContainsKey(inputAxis.Axis))
                    continue;

                _observerAxisList.Add(inputAxis.Axis);
                _axisToValueDic.Add(inputAxis.Axis, 0);
            }
        }

        public void RegisterInputReceiver(IInputReceiver inputReceiver)
        {
            if (inputReceiver is T type)
            {
                if (_inputReceiverList.Contains(type))
                    return;
                _inputReceiverList.Add(type);
            }
        }

        public void UnRegisterInputReceiver(IInputReceiver inputReceiver)
        {
            if (inputReceiver is T type)
            {
                _inputReceiverList.Remove(type);
            }
        }

        protected virtual void DetectInputKeyCode()
        {
            int count = _observerKeyCodeList.Count;
            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
            {
                var keyCode = _observerKeyCodeList[i];
                bool keyDown = Input.GetKeyDown(keyCode);
                bool keyUp = Input.GetKeyUp(keyCode);

                if (!keyDown && !keyUp)
                    continue;

                if (!_keyCodeToTriggerCommandListDic.
                    TryGetValue(keyCode, out var keyCodeTriggerCommand))
                    continue;

                if (keyDown)
                {
                    for (int j = 0; j < keyCodeTriggerCommand.KeyDownCommandList.Count; j++)
                    {
                        OnKeyDown(keyCode, keyCodeTriggerCommand.KeyDownCommandList[j]);
                    }

                    if (!_holdKeyCodeList.Contains(keyCode))
                    {
                        _holdKeyCodeList.Add(keyCode);
                    }
                }
                if (keyUp)
                {
                    for (int j = 0; j < keyCodeTriggerCommand.KeyUpCommandList.Count; j++)
                    {
                        OnKeyUp(keyCode, keyCodeTriggerCommand.KeyUpCommandList[j]);
                    }

                    _holdKeyCodeList.Remove(keyCode);
                }
            }

            if (_holdKeyCodeList.Count > 0)
            {
                for (int i = 0; i < _holdKeyCodeList.Count; i++)
                {
                    var keyCode = _holdKeyCodeList[i];
                    if (!_keyCodeToTriggerCommandListDic.
                        TryGetValue(keyCode, out var keyCodeTriggerCommand))
                        continue;

                    for (int j = 0; j < keyCodeTriggerCommand.KeyHoldCommandList.Count; j++)
                    {
                        OnKeyHold(keyCode, keyCodeTriggerCommand.KeyHoldCommandList[j]);
                    }
                }
            }
        }

        protected virtual void DetectInputAxis()
        {
            int count = _observerAxisList.Count;
            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
            {
                var axis = _observerAxisList[i];
                var newValue = Input.GetAxis(axis);
                if (_axisToValueDic[axis] != newValue)
                {
                    _axisToValueDic[axis] = newValue;
                    OnAxisValueChanged(axis, newValue);
                }
            }
        }

        protected virtual void OnKeyDown(KeyCode keyCode, string command) { }
        protected virtual void OnKeyUp(KeyCode keyCode, string command) { }
        protected virtual void OnKeyHold(KeyCode keyCode, string command) { }

        protected virtual void OnAxisValueChanged(string axis, float value) { }
    }
}
