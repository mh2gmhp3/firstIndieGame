using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InputModule
{
    public class GameInputReceiver : IInputReceiver
    {
        public delegate void KeyboardInput(KeyCode keyCode, string command);

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
    }
}
