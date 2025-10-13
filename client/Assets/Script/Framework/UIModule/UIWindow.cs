using Extension;
using Logging;
using UnityEngine;

namespace UIModule
{
    /// <summary>
    /// UI主要介面
    /// </summary>
    public abstract class UIWindow : UIBase
    {
        /// <summary>
        /// 視窗名稱
        /// </summary>
        public string WindowName;
        protected Canvas _canvas;

        protected sealed override void DoInit()
        {
            _canvas = gameObject.GetComponent<Canvas>();
            _canvas.overrideSorting = true;
            _canvas.transform.localScale = Vector3.one;

            _rectTransform.SetStretch();

            OnInit();
        }

        /// <summary>
        /// 設置階層
        /// </summary>
        /// <param name="sortingLayerName"></param>
        /// <param name="sortingOrder"></param>
        public void SetSortingLayer(string sortingLayerName, int sortingOrder)
        {
            if (_canvas == null)
            {
                Log.LogError("Canvas is null");
                return;
            }

            _canvas.sortingLayerName = sortingLayerName;
            _canvas.sortingOrder = sortingOrder;
        }

        /// <summary>
        /// 開啟
        /// </summary>
        /// <param name="uiData"></param>
        public void Open(IUIData uiData)
        {
            gameObject.SetActive(true);
            SetUIData(uiData);
            DoOpen(uiData);
        }

        /// <summary>
        /// 關閉
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
            ClearUIData();
            DoClose();
        }

        /// <summary>
        /// 設定顯示
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (visible)
            {
                UISystem.OpenUIWindow(WindowName, _uiData);
            }
            else
            {
                UISystem.CloseUIWindow(WindowName);
            }
        }

        /// <summary>
        /// 開啟時呼叫
        /// </summary>
        /// <param name="uiData"></param>
        protected virtual void DoOpen(IUIData uiData) { }
        /// <summary>
        /// 關閉時呼叫
        /// </summary>
        protected virtual void DoClose() { }

        /// <summary>
        /// 初始化時呼叫
        /// </summary>
        protected virtual void OnInit() { }
    }
}
