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
        public void Notify(UIData uiData);
    }

    /// <summary>
    /// 介面使用的資料
    /// </summary>
    public abstract class UIData
    {
        ~UIData()
        {
            _notifyReceiverList.Clear();
            _notifyReceiverList = null;
        }

        /// <summary>
        /// 所有要接收通知者
        /// </summary>
        private List<IUIDataNotifyReceiver> _notifyReceiverList = new List<IUIDataNotifyReceiver>();

        /// <summary>
        /// 加入接收通知者
        /// </summary>
        /// <param name="notifyReceiver"></param>
        public void AddNotifyReceiver(IUIDataNotifyReceiver notifyReceiver)
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
        public void RemoveNotifyReceiver(IUIDataNotifyReceiver notifyReceiver)
        {
            if (notifyReceiver == null)
                return;

            _notifyReceiverList.Remove(notifyReceiver);
        }

        /// <summary>
        /// 通知所有接收者
        /// </summary>
        public void Notify()
        {
            for (int i = 0; i < _notifyReceiverList.Count; i++)
            {
                var receiver = _notifyReceiverList[i];
                if (receiver == null)
                    continue;

                receiver.Notify(this);
            }
        }
    }
}