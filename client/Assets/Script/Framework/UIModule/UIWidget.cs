using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIModule
{
    /// <summary>
    /// Widget事件資料
    /// </summary>
    public struct WidgetEventData
    {
        /// <summary>
        /// 事件Id
        /// </summary>
        public int EventId;
        /// <summary>
        /// 事件物件(自行轉型)
        /// </summary>
        public object Object;
    }

    public delegate void WidgetEvent(WidgetEventData eventData);

    /// <summary>
    /// UI小型介面
    /// </summary>
    public abstract class UIWidget : UIBase
    {
        /// <summary>
        /// Widget事件
        /// </summary>
        private WidgetEvent _widgetEvent = null;

        public void Awake()
        {
            Init();
        }

        /// <summary>
        /// 註冊Widget事件
        /// </summary>
        /// <param name="widgetEvent"></param>
        public void RegisterWidgetEvent(WidgetEvent widgetEvent)
        {
            _widgetEvent += widgetEvent;
        }

        /// <summary>
        /// 呼叫所有註冊事件
        /// </summary>
        /// <param name="eventData"></param>
        public void InvokeWidgetEvent(WidgetEventData eventData)
        {
            if (_widgetEvent == null)
                return;

            _widgetEvent.Invoke(eventData);
        }

        /// <summary>
        /// 設定UIData
        /// </summary>
        /// <param name="data"></param>
        public void SetData(IUIData data)
        {
            Init();
            SetUIData(data);
            DoSetData();
        }

        public void ClearData()
        {
            ClearUIData();
        }

        /// <summary>
        /// 設定顯示
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (visible)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        protected sealed override void DoNotify(IUIData data, IUIDataNotifyInfo notifyInfo)
        {
            if (_uiData != data)
                return;

            OnUIDataNotify(notifyInfo);
        }

        /// <summary>
        /// 於設定UIData時呼叫
        /// </summary>
        protected abstract void DoSetData();

        /// <summary>
        /// 接收UIData通知
        /// </summary>
        /// <param name="notifyInfo"></param>
        protected abstract void OnUIDataNotify(IUIDataNotifyInfo notifyInfo);
    }
}
