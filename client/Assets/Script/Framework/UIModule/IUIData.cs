using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIModule
{
    /// <summary>
    /// 接收UIData通知者
    /// </summary>
    public interface IUIDataNotifyReceiver
    {
        /// <summary>
        /// UIData通知
        /// </summary>
        /// <param name="uiData"></param>
        public void Notify(IUIData uiData, IUIDataNotifyInfo notifyInfo);
    }

    /// <summary>
    /// 通知資訊
    /// </summary>
    public struct IUIDataNotifyInfo
    {
        /// <summary>
        /// 辨識用 一般使用
        /// </summary>
        public int Identify;
        /// <summary>
        /// 如果有int處理不了的詳細資訊放這
        /// </summary>
        public object Object;
    }

    /// <summary>
    /// 介面使用的資料
    /// </summary>
    public abstract class IUIData
    {
        ~IUIData()
        {
            _notifyReceiverList.Clear();
            _notifyReceiverList = null;
        }

        /// <summary>
        /// 所有要接收通知者
        /// </summary>
        private List<IUIDataNotifyReceiver> _notifyReceiverList = new List<IUIDataNotifyReceiver>();

        /// <summary>
        /// 通知所有接收者
        /// </summary>
        public void Notify(IUIDataNotifyInfo notifyInfo)
        {
            for (int i = 0; i < _notifyReceiverList.Count; i++)
            {
                var receiver = _notifyReceiverList[i];
                if (receiver == null)
                    continue;

                receiver.Notify(this, notifyInfo);
            }
        }

        /// <summary>
        /// 加入接收通知者
        /// </summary>
        /// <param name="notifyReceiver"></param>
        private void AddNotifyReceiver(IUIDataNotifyReceiver notifyReceiver)
        {
            if (notifyReceiver == null)
                return;

            if (_notifyReceiverList.Contains(notifyReceiver))
                return;

            _notifyReceiverList.Add(notifyReceiver);
        }

        /// <summary>
        /// 移除接收通知者
        /// </summary>
        /// <param name="notifyReceiver"></param>
        private void RemoveNotifyReceiver(IUIDataNotifyReceiver notifyReceiver)
        {
            if (notifyReceiver == null)
                return;

            _notifyReceiverList.Remove(notifyReceiver);
        }

        /// <summary>
        /// 將接收者添加至Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="receiver"></param>
        public static void AddNotifyReceiver(IUIData data, IUIDataNotifyReceiver receiver)
        {
            if (data == null)
            {
                //允許不註冊資料
                return;
            }

            if (receiver == null)
            {
                Log.LogError($"{data.GetType().Name}.AddNotifyReceiver Error : receiver is null");
                return;
            }

            data.AddNotifyReceiver(receiver);
        }

        /// <summary>
        /// 將接收者從Data移除
        /// </summary>
        /// <param name="data"></param>
        /// <param name="receiver"></param>
        public static void RemoveNotifyReceiver(IUIData data, IUIDataNotifyReceiver receiver)
        {
            if (data == null)
            {
                // 因為允許不註冊資料 這邊不需要跳警告
                return;
            }

            if (receiver == null)
            {
                Log.LogError($"{data.GetType().Name}.RemoveNotifyReceiver Error : receiver is null");
                return;
            }

            data.RemoveNotifyReceiver(receiver);
        }
    }
}