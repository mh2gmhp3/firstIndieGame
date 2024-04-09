using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InputModule
{
    public class GameInputReceiver : IInputReceiver
    {
        public delegate void KeyboardInput(KeyCode keyCode, string command);
        public delegate void AxisValueChangedEvent(List<float> axisValue);

        public KeyboardInput OnKeyDown;
        public KeyboardInput OnKeyUp;
        public KeyboardInput OnKeyHold;

        public GameInputReceiver(
            KeyboardInput onKeyDown,
            KeyboardInput onKeyUp,
            KeyboardInput onKeyHold)
        {
            OnKeyDown = onKeyDown;
            OnKeyUp = onKeyUp;
            OnKeyHold = onKeyHold;
        }

        /// <summary>
        /// 註冊Axis變動事件 使用AxisList 存在於AxisList內任意值變動後將傳入整個AxisList的所有Value
        /// </summary>
        /// <param name="axisList"></param>
        /// <param name="axisValueChangedEvent"></param>
        public void RegisterAxisValueChangedEvent(
            List<string> axisList,
            AxisValueChangedEvent axisValueChangedEvent)
        {

        }

        /// <summary>
        /// 負責接收Axis變動
        /// </summary>
        /// <param name="axisList"></param>
        /// <param name="axisToValueDic"></param>
        public void OnAxisValueChanged(
            List<string> axisList,
            Dictionary<string, float> axisToValueDic)
        {

        }
    }
}
