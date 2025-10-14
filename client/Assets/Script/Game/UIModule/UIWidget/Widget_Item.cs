using DataModule;
using Logging;

namespace UIModule.Game
{
    public partial class Widget_Item : UIWidget
    {
        public class EmptyData : IUIData
        {
            //可以加入空模式 不同顯示類型
        }

        protected override void DoSetData()
        {
            OnUIDataNotify(default);
        }

        protected override void OnUIDataNotify(IUIDataNotifyInfo notifyInfo)
        {
            if (_uiData == null)
            {
                SetVisible(false);
            }

            if (_uiData is UIItemData itemData)
            {
                SetVisible(true);
                SetView(itemData);
            }
            else if (_uiData is EmptyData emptyData)
            {
                Clear();
            }
            else
            {
                Log.LogError("Widget_Item OnUIDataNotify Error, Data unknown");
            }
        }

        private void SetView(UIItemData itemData)
        {
            Text_Content.text = $"{itemData.Id}_{itemData.SettingId}_{itemData.Count}";
        }

        private void Clear()
        {
            Text_Content.text = string.Empty;
        }
    }
}
