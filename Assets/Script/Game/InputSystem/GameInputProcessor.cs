using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputModule
{
    public class GameInputProcessor : BaseInputProcessor<GameInputReceiver>
    {
        protected override void OnKeyDown(KeyCode keyCode, string command)
        {
            for (int i = 0; i < _inputReceiverList.Count; i++)
            {
                if (_inputReceiverList[i].OnKeyDown == null)
                    continue;

                _inputReceiverList[i].OnKeyDown.Invoke(keyCode, command);
            }
        }

        protected override void OnKeyUp(KeyCode keyCode, string command)
        {
            for (int i = 0; i < _inputReceiverList.Count; i++)
            {
                if (_inputReceiverList[i].OnKeyUp == null)
                    continue;

                _inputReceiverList[i].OnKeyUp.Invoke(keyCode, command);
            }
        }
    }
}
