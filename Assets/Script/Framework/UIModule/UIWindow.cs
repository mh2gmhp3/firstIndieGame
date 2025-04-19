using Extension;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIModule
{
    /// <summary>
    /// UI主要介面
    /// </summary>
    public abstract class UIWindow : UIBase
    {
        public string WindowName;
        protected Canvas _canvas;

        protected override void DoInit()
        {
            _canvas = gameObject.GetComponent<Canvas>();
            _canvas.overrideSorting = true;

            _rectTransform.SetStretch();
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
    }
}
