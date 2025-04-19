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

        /// <summary>
        /// 設定顯示
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (visible)
            {
                Open(_uiData);
            }
            else
            {
                Close();
            }
        }

        /// <summary>
        /// 於設定UIData時呼叫
        /// </summary>
        protected abstract void DoSetData();
    }
}
