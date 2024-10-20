using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.ComponentUtility;

namespace UIModule
{
    public class UIBase : MonoBehaviour, IUIDataNotifyReceiver
    {
        private UIData _uiData = null;

        [SerializeField]
        protected ObjectReferenceDatabase _objectReferenceDb = new ObjectReferenceDatabase();

#if UNITY_EDITOR
        public ObjectReferenceDatabase ObjectReferenceDb => _objectReferenceDb;

#endif

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
