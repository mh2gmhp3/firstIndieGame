using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIModule
{
    public class SimpleScrollerCellView : MonoBehaviour
    {
        [SerializeField]
        private string _identity;
        [SerializeField]
        private List<UIWidget> _uiWidgetList = new List<UIWidget>();

        public string Identity => _identity;

        public int WidgetCount => _uiWidgetList.Count;

        public void SetData(IUIData data, int index)
        {
            if (index < 0 || index >= _uiWidgetList.Count)
                return;

            _uiWidgetList[index].SetData(data);
        }

        public void DoRecycle()
        {
            for (int i = 0; i < _uiWidgetList.Count; i++)
            {
                _uiWidgetList[i].ClearData();
                _uiWidgetList[i].ClearWidgetEvent();
            }
        }

        public void RegisterWidgetEvent(WidgetEvent widgetEvent)
        {
            for (int i = 0; i < _uiWidgetList.Count; i++)
            {
                _uiWidgetList[i].RegisterWidgetEvent(widgetEvent);
            }
        }
    }
}
