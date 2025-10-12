using DataModule;
using Extension;
using System.Collections.Generic;

namespace UIModule.Game
{
    public partial class Window_ItemSelect : UIWindow
    {
        public class UISelectItemDataContainer : IUIData, IScrollerControllerDataGetter
        {
            public List<UIItemData> ItemDataList = new List<UIItemData>();
            private int _cellWidgetCount;

            public UISelectItemDataContainer(List<UIItemData> itemDataList)
            {
                ItemDataList.AddRange(itemDataList);
            }

            public void SetScroller(SimpleScrollerController scroller)
            {
                _cellWidgetCount = scroller.GetScrollerCellWidgetCount("");
            }

            public int GetCellCount()
            {
                return ItemDataList.Count / _cellWidgetCount;
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

        protected override void DoOpen(IUIData uiData)
        {
            if (uiData is UISelectItemDataContainer dataContainer)
            {
                dataContainer.SetScroller(SimpleScrollerController_ItemScroller);
                SimpleScrollerController_ItemScroller.SetDataGetter(dataContainer);
            }
        }

        protected override void DoClose()
        {

        }

        protected override void DoNotify(IUIData data, IUIDataNotifyInfo notifyInfo)
        {

        }
    }
}
