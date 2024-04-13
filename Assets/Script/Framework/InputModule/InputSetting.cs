using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputModule
{
    public enum KeyCodeTriggerType
    {
        KeyDown,
        KeyUp,
        KeyHold
    }

    /// <summary>
    /// KeyCode觸發設定
    /// </summary>
    [Serializable]
    public class KeyCodeTriggerSetting
    {
        public KeyCode KeyCode;
        public KeyCodeTriggerType Type;
    }

    /// <summary>
    /// 設定各命令的KeyCode
    /// </summary>
    [Serializable]
    public class InputCommand
    {
        public string Command;
        public List<KeyCodeTriggerSetting> AssignKeyCodeTriggerSettingList;
    }

    /// <summary>
    /// 要接收的Unity輸入軸
    /// </summary>
    [Serializable]
    public class InputAxis
    {
        public string Axis;
    }

    [CreateAssetMenu(fileName = "InputSetting", menuName = "InputSystem/InputSetting")]
    public class InputSetting : ScriptableObject
    {
        public List<InputCommand> InputCommandList =
           new List<InputCommand>();

        public List<InputAxis> InputAxisList =
           new List<InputAxis>();
    }
}
