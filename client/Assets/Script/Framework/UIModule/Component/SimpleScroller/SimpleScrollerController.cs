using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace UIModule
{
    public interface IScrollerControllerDataGetter
    {
        /// <summary>
        /// 獲取Cell數量 自行對應資料數量計算
        /// </summary>
        /// <returns></returns>
        public int GetCellCount();
        /// <summary>
        /// 獲取此Index的Cell辨識
        /// </summary>
        /// <param name="cellIndex"></param>
        /// <returns></returns>
        public string GetCellIdentity(int cellIndex);
        /// <summary>
        /// 獲取此Cell中第幾個Widget的資料
        /// </summary>
        /// <param name="cellIndex"></param>
        /// <param name="widgetIndex"></param>
        /// <returns></returns>
        public IUIData GetUIData(int cellIndex, int widgetIndex);
    }

    [RequireComponent(typeof(ScrollRect))]
    /// <summary>
    /// 簡易Scroller 處理設定資料行為
    /// </summary>
    public class SimpleScrollerController : MonoBehaviour
    {
        [SerializeField]
        private List<SimpleScrollerCellView> _scrollViewTemplateInfoList = new List<SimpleScrollerCellView>();

        private IScrollerControllerDataGetter _scrollerControllerDataGetter;

        private List<SimpleScrollerCellView> _usingCellView = new List<SimpleScrollerCellView>();
        private Dictionary<string, Queue<SimpleScrollerCellView>> _identityToCellViewPool =
            new Dictionary<string, Queue<SimpleScrollerCellView>>();

        private WidgetEvent _scrollerWidgetEvent;

        private ScrollRect _scroller;
        public ScrollRect Scroller
        {
            get
            {
                if (_scroller == null)
                {
                    // 有加RequireComponent基本上一定會有
                    _scroller = GetComponent<ScrollRect>();
                }

                return _scroller;
            }
        }

        /// <summary>
        /// 設定資料獲取者
        /// </summary>
        /// <param name="dataGetter"></param>
        public void SetDataGetter(IScrollerControllerDataGetter dataGetter)
        {
            if (dataGetter == null)
            {
                Log.LogError("SimpleScroller SetData Error, dataGetter is null");
                return;
            }

            _scrollerControllerDataGetter = dataGetter;
            Reload();
        }

        public void Reload()
        {
            // 全不回收
            for (int i = _usingCellView.Count - 1; i >= 0; i--)
            {
                ReleaseCell(_usingCellView[i]);
            }

            // 重新產生 TODO 目前是資料有多少就產多少個 資料多起來會有問題 要能夠指生成需要看到的就可以了
            var cellCount = _scrollerControllerDataGetter.GetCellCount();
            for (int cellIndex = 0; cellIndex < cellCount; cellIndex++)
            {
                AddCell(cellIndex);
            }
        }

        public void RegisterScrollerWidgetEvent(WidgetEvent widgetEvent)
        {
            _scrollerWidgetEvent = widgetEvent;
        }

        private void AddCell(int cellIndex)
        {
            var identity = _scrollerControllerDataGetter.GetCellIdentity(cellIndex);
            var cellView = GetCell(identity);
            if (cellView == null)
            {
                Log.LogError($"SimpleScrollerController AddCell Error, cell not found, " +
                    $"{gameObject.name}, CellIndex:{cellIndex} Identity:{identity}");
                return;
            }

            _usingCellView.Add(cellView);
            cellView.gameObject.SetActive(true);
            cellView.transform.SetSiblingIndex(cellIndex);
            var widgetCount = cellView.WidgetCount;
            for (int widgetIndex = 0; widgetIndex < widgetCount; widgetIndex++)
            {
                var data = _scrollerControllerDataGetter.GetUIData(cellIndex, widgetIndex);
                cellView.SetData(data, widgetIndex);
                cellView.RegisterWidgetEvent(_scrollerWidgetEvent);
            }
        }

        private SimpleScrollerCellView GetCell(string identity)
        {
            if (_identityToCellViewPool.TryGetValue(identity, out var pool))
            {
                if (pool.Count > 0)
                    return pool.Dequeue();
            }

            var cellViewTemplate = GetTemplateCell(identity);
            // 有可能 null
            if (cellViewTemplate == null)
                return null;

            var cellView = Instantiate(cellViewTemplate, Scroller.content);
            return cellView;
        }

        private SimpleScrollerCellView GetTemplateCell(string identity)
        {
            for (int i = 0; i < _scrollViewTemplateInfoList.Count; i++)
            {
                if (_scrollViewTemplateInfoList[i].Identity == identity)
                    return _scrollViewTemplateInfoList[i];
            }

            return null;
        }

        private void ReleaseCell(SimpleScrollerCellView cellView)
        {
            cellView.DoRecycle();
            _usingCellView.Remove(cellView);
            cellView.gameObject.SetActive(false);
            cellView.transform.SetAsLastSibling();
            ReleaseCell(cellView.Identity, cellView);
        }

        private void ReleaseCell(string identity, SimpleScrollerCellView cellView)
        {
            if (!_identityToCellViewPool.TryGetValue(identity, out var pool))
            {
                pool = new Queue<SimpleScrollerCellView>();
                _identityToCellViewPool.Add(identity, pool);
            }
            pool.Enqueue(cellView);
        }
    }
}
