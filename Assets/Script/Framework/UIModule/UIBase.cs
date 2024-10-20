using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.ComponentUtility;

namespace UIModule
{
    public class UIBase : MonoBehaviour, IUIDataNotifyReceiver
    {
        /// <summary>
        /// 是否初始化過
        /// </summary>
        private bool _inited = false;

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

        public void Init()
        {
            if (_inited)
                return;

            DoInit();

            _inited = true;
        }

        public virtual void DoInit() { }

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
