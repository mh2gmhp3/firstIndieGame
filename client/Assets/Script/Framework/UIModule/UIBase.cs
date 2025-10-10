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
        protected IUIData _uiData = null;

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

        public void Notify(IUIData uiData, IUIDataNotifyInfo notifyInfo)
        {
            DoNotify(uiData, notifyInfo);
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

            InitComponentReference();
            InitComponentEvent();
            DoInit();

            _inited = true;
        }

        /// <summary>
        /// 設定UIData
        /// </summary>
        /// <param name="uiData"></param>
        protected void SetUIData(IUIData uiData)
        {
            // 如果之前有註冊要清掉
            if (_uiData != null)
                IUIData.RemoveNotifyReceiver(_uiData, this);

            _uiData = uiData;
            IUIData.AddNotifyReceiver(_uiData, this);
        }

        /// <summary>
        /// 清空UIData
        /// </summary>
        protected void ClearUIData()
        {
            IUIData.RemoveNotifyReceiver(_uiData, this);
            _uiData = null;
        }

        /// <summary>
        /// 初始化時呼叫
        /// </summary>
        protected virtual void DoInit() { }

        /// <summary>
        /// 初始化Component參考
        /// </summary>
        protected abstract void InitComponentReference();
        /// <summary>
        /// 初始化Component事件
        /// </summary>
        protected abstract void InitComponentEvent();

        /// <summary>
        /// 接收UIData通知
        /// </summary>
        /// <param name="data"></param>
        /// <param name="notifyInfo"></param>
        protected abstract void DoNotify(IUIData data, IUIDataNotifyInfo notifyInfo);
    }
}
