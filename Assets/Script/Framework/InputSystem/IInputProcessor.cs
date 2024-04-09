using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputModule
{
    public interface IInputProcessor
    {
        public void DetectInput();

        public void SetInputSetting(InputSetting inputSetting);

        public void RegisterInputReceiver(IInputReceiver inputReceiver);
        public void UnRegisterInputReceiver(IInputReceiver inputReceiver);
    }
}
