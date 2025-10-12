using DataModule;
using Logging;

namespace UIModule.Game
{
    public partial class Widget_Item : UIWidget
    {
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
            else
            {
                Log.LogError("Widget_Item OnUIDataNotify Error, Data unknown");
            }
        }

        private void SetView(UIItemData itemData)
        {
            Text_Content.text = $"{itemData.Id}_{itemData.SettingId}_{itemData.Count}";
        }
    }
}
