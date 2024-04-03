using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputModule
{
    [Serializable]
    public class KeyboardInputSetting
    {
        public KeyCode KeyCode;
        public string Command;
    }

    [CreateAssetMenu(fileName = "InputSetting", menuName = "InputSystem/InputSetting")]
    public class InputSetting : ScriptableObject
    {
        public List<KeyboardInputSetting> KeyboardInputSettingList =
           new List<KeyboardInputSetting>();
    }
}
