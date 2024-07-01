using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIModule
{
    public interface IUIDataNotifyReceiver
    {
        public void Notify(UIData uiData);
    }

    public class UIData
    {
        ~UIData()
        {
            _notifyReceiverList.Clear();
            _notifyReceiverList = null;
        }

        private List<IUIDataNotifyReceiver> _notifyReceiverList = new List<IUIDataNotifyReceiver>();

        public void AddNotifyReceiver(IUIDataNotifyReceiver notifyReceiver)
        {
            if (notifyReceiver == null)
                return;

            if (_notifyReceiverList.Contains(notifyReceiver))
                return;

            _notifyReceiverList.Add(notifyReceiver);
        }

        public void RemoveNotifyReceiver(IUIDataNotifyReceiver notifyReceiver)
        {
            if (notifyReceiver == null)
                return;

            _notifyReceiverList.Remove(notifyReceiver);
        }

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