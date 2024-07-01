using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIModule
{
    public class UIWindows : UIBase
    {
        protected Canvas _canvas;

        public override void Init()
        {
            _canvas = gameObject.GetComponent<Canvas>();
        }

        public void SetSortingLayer(string sortingLayerName, int sortingOrder)
        {
            if (_canvas == null)
            {
                Log.LogError("Canvas is null");
                return;
            }

            _canvas.overrideSorting = true;
            _canvas.sortingLayerName = sortingLayerName;
            _canvas.sortingOrder = sortingOrder;
        }
    }
}
