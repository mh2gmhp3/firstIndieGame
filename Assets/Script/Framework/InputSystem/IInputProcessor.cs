using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InputModule
{
    /// <summary>
    /// 輸入處理者介面
    /// </summary>
    public interface IInputProcessor
    {
        /// <summary>
        /// 偵測輸入 於Update時呼叫
        /// </summary>
        public void DetectInput();

        /// <summary>
        /// 設定輸入設定
        /// </summary>
        /// <param name="inputSetting"></param>
        public void SetInputSetting(InputSetting inputSetting);

        /// <summary>
        /// 註冊輸入接收者
        /// </summary>
        /// <param name="inputReceiver"></param>
        public void RegisterInputReceiver(IInputReceiver inputReceiver);
        /// <summary>
        /// 註冊輸入接接收者
        /// </summary>
        /// <param name="inputReceiver"></param>
        public void UnRegisterInputReceiver(IInputReceiver inputReceiver);
    }
}
