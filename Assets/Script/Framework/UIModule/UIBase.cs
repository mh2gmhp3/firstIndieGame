using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIModule
{
    public class UIBase : MonoBehaviour, IUIDataNotifyReceiver
    {
        private UIData _uiData = null;

        #region IUIDataNotifyReceiver

        public void Notify(UIData uiData)
        {

        }

        #endregion

        public virtual void Init() { }
        public void Open(UIData uiData)
        {
            gameObject.SetActive(true);
            DoOpen(uiData);
        }
        public void Close()
        {
            gameObject.SetActive(false);
            DoClose();
        }

        protected virtual void DoOpen(UIData uiData) { }
        protected virtual void DoClose() { }
    }
}
