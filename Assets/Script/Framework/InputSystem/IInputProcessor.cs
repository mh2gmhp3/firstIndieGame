using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputModule
{
    public interface IInputProcessor
    {
        public void DetectKeyboardInput();

        public void SetInputSetting(InputSetting inputSetting);

        public void RegisterInputReceiver(IInputReceiver inputReceiver);
        public void UnRegisterInputReceiver(IInputReceiver inputReceiver);
    }
}
