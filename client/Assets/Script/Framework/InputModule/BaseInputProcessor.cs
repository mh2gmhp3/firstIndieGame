using System.Collections.Generic;
using UnityEngine;

namespace InputModule
{
    /// <summary>
    /// 基本輸入處理者
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseInputProcessor<T> : IInputProcessor
        where T : IInputReceiver
    {
        /// <summary>
        /// 運行時的各KeyCode對應觸發行為的命令
        /// </summary>
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

        /// <summary>
        /// 監聽的KeyCodeList
        /// </summary>
        protected List<KeyCode> _observerKeyCodeList = new List<KeyCode>();
        /// <summary>
        /// 監聽的KeyCode觸發命令
        /// </summary>
        protected Dictionary<KeyCode, RuntimeKeyCodeTriggerCommand> _keyCodeToTriggerCommandListDic =
              new Dictionary<KeyCode, RuntimeKeyCodeTriggerCommand>();
        /// <summary>
        /// 當前按著的KeyCode
        /// </summary>
        protected List<KeyCode> _holdKeyCodeList = new List<KeyCode>();

        /// <summary>
        /// 監聽的AxisList
        /// </summary>
        protected List<string> _observerAxisList = new List<string>();
        /// <summary>
        /// Axis的值
        /// </summary>
        protected Dictionary<string, float> _axisToValueDic =
              new Dictionary<string, float>();
        /// <summary>
        /// 當前偵有變動的Axis
        /// </summary>
        protected List<string> _changedAxisList = new List<string>();

        /// <summary>
        /// 輸入接收者列表
        /// </summary>
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

                    var commandList = keyCodeTriggerCommand.
                        GetCommandListByType(keyCodeTriggerSetting.Type);

                    if (commandList.Contains(inputCommand.Command))
                        continue;

                    commandList.Add(inputCommand.Command);
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
            if (inputReceiver is T typeInputReceiver)
            {
                if (_inputReceiverList.Contains(typeInputReceiver))
                    return;
                _inputReceiverList.Add(typeInputReceiver);
            }
        }

        public void UnRegisterInputReceiver(IInputReceiver inputReceiver)
        {
            if (inputReceiver is T typeInputReceiver)
            {
                _inputReceiverList.Remove(typeInputReceiver);
            }
        }

        /// <summary>
        /// 偵測輸入的KeyCode
        /// </summary>
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

        /// <summary>
        /// 偵測輸入的Axis
        /// </summary>
        protected virtual void DetectInputAxis()
        {
            int count = _observerAxisList.Count;
            if (count == 0)
                return;

            _changedAxisList.Clear();
            for (int i = 0; i < count; i++)
            {
                var axis = _observerAxisList[i];
                var newValue = Input.GetAxis(axis);
                if (_axisToValueDic[axis] != newValue)
                {
                    _axisToValueDic[axis] = newValue;
                    _changedAxisList.Add(axis);
                }
            }
            if (_changedAxisList.Count > 0)
            {
                OnAxisValueChanged(_changedAxisList);
            }
        }

        /// <summary>
        /// 當KeyCode按下時呼叫
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="command"></param>
        protected virtual void OnKeyDown(KeyCode keyCode, string command) { }
        /// <summary>
        /// 當KeyCode放開時呼叫
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="command"></param>
        protected virtual void OnKeyUp(KeyCode keyCode, string command) { }
        /// <summary>
        /// 當KeyCode按著時呼叫
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="command"></param>
        protected virtual void OnKeyHold(KeyCode keyCode, string command) { }

        /// <summary>
        /// 於Axis變動時呼叫 傳入當偵所有變動的Axis
        /// </summary>
        /// <param name="axisList"></param>
        protected virtual void OnAxisValueChanged(List<string> axisList) { }
    }
}
