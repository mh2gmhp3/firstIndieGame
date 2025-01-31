using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.ComponentUtility;

namespace UIModule
{
    /// <summary>
    /// UI基礎類別
    /// </summary>
    public abstract class UIBase : MonoBehaviour, IUIDataNotifyReceiver
    {
        /// <summary>
        /// 是否初始化過
        /// </summary>
        private bool _inited = false;

        /// <summary>
        /// 介面主要資料
        /// </summary>
        private UIData _uiData = null;

        /// <summary>
        /// RectTransform
        /// </summary>
        protected RectTransform _rectTransform;

        /// <summary>
        /// 介面ReferenceDB 記錄所有Reference
        /// </summary>
        [SerializeField]
        protected ObjectReferenceDatabase _objectReferenceDb = new ObjectReferenceDatabase();

#if UNITY_EDITOR
        /// <summary>
        /// 介面ReferenceDB 記錄所有Reference for Editor
        /// </summary>
        public ObjectReferenceDatabase ObjectReferenceDb => _objectReferenceDb;

#endif

        #region IUIDataNotifyReceiver

        public void Notify(UIData uiData)
        {

        }

        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            if (_inited)
                return;

            _rectTransform = this.transform as RectTransform;

            InitComponentRefreence();
            InitComponentEvent();
            DoInit();

            _inited = true;
        }

        /// <summary>
        /// 開啟
        /// </summary>
        /// <param name="uiData"></param>
        public void Open(UIData uiData)
        {
            gameObject.SetActive(true);
            DoOpen(uiData);
        }

        /// <summary>
        /// 關閉
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
            DoClose();
        }

        /// <summary>
        /// 初始化時呼叫
        /// </summary>
        protected virtual void DoInit() { }
        /// <summary>
        /// 開啟時呼叫
        /// </summary>
        /// <param name="uiData"></param>
        protected virtual void DoOpen(UIData uiData) { }
        /// <summary>
        /// 關閉時呼叫
        /// </summary>
        protected virtual void DoClose() { }

        /// <summary>
        /// 初始化Component參考
        /// </summary>
        protected abstract void InitComponentRefreence();
        /// <summary>
        /// 初始化Compnent事件
        /// </summary>
        protected abstract void InitComponentEvent();
    }
}
