using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    /// <summary>
    /// 可用來自定義UnityUpdate行為的目標 自行收集後處理
    /// 例:透過收集所有要Update的不管是不是Mono的目標 透過一個能夠接收Mono生命週期的管理者呼叫
    /// </summary>
    public interface IUpdateTarget
    {
        void DoUpdate();
        void DoFixedUpdate();
        void DoLateUpdate();
        void DoOnGUI();
        void DoDrawGizmos();
    }
}
