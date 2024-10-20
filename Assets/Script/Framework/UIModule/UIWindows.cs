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
    public class UIWindows : UIBase
    {
        protected Canvas _canvas;

        protected override void DoInit()
        {
            _canvas = gameObject.GetComponent<Canvas>();
            _canvas.overrideSorting = true;
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
    }
}
