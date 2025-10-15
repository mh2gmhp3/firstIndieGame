using DataModule;
using Extension;
using System;
using System.Collections.Generic;

namespace UIModule.Game
{
    public partial class Window_ItemSelect : UIWindow
    {
        public class UISelectItemDataContainer : IUIData, IScrollerControllerDataGetter
        {
            public List<ItemData> ItemDataList = new List<ItemData>();
            public Action<ItemData> SelectedEvent;
            private int _cellWidgetCount;

            public UISelectItemDataContainer(List<ItemData> itemDataList, Action<ItemData> selectedEvent = null)
            {
                ItemDataList.AddRange(itemDataList);
                SelectedEvent = selectedEvent;
            }

            public void SetScroller(SimpleScrollerController scroller)
            {
                _cellWidgetCount = scroller.GetScrollerCellWidgetCount("");
            }

            public int GetCellCount()
            {
                return (int)Math.Ceiling(ItemDataList.Count / (float)_cellWidgetCount);
            }

            public string GetCellIdentity(int cellIndex)
            {
                return "";
            }

            public IUIData GetUIData(int cellIndex, int widgetIndex)
            {
                var index = cellIndex * _cellWidgetCount + widgetIndex;
                if (!ItemDataList.TryGet(index, out var data))
                    return null;
                return data;
            }
        }

        private UISelectItemDataContainer _container;
        private ItemData _selectedData = null;

        protected override void DoOpen(IUIData uiData)
        {
            if (uiData is UISelectItemDataContainer dataContainer)
            {
                _container = dataContainer;;
                _container.SetScroller(SimpleScrollerController_ItemScroller);
                SimpleScrollerController_ItemScroller.SetDataGetter(_container);
                SetSelectedData(null);
            }
        }

        protected override void DoClose()
        {
            _container = null;
            _selectedData = null;
        }

        protected override void DoNotify(IUIData data, IUIDataNotifyInfo notifyInfo)
        {

        }

        private void SetSelectedData(ItemData itemData)
        {
            if (itemData == null)
            {
                _selectedData = null;
                GameObject_Item_Info_Root.SetActive(false);
                return;
            }

            _selectedData = itemData;
            GameObject_Item_Info_Root.SetActive(true);
            Widget_Item_Select_Item.SetData(_selectedData);
        }
    }
}
