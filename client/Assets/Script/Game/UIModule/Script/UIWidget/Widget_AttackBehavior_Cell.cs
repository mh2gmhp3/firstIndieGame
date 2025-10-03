using static UIModule.Game.Window_AttackBehaviorEdit;

namespace UIModule.Game
{
    public partial class Widget_AttackBehavior_Cell : UIWidget
    {
        protected override void DoSetData()
        {
            SetVisible(true);
            OnUIDataNotify(default);
        }

        protected override void OnUIDataNotify(IUIDataNotifyInfo notifyInfo)
        {
            var data = _uiData as UIAttackBehaviorData;
            Text_Content.text = $"Id:{data.RawData.Id}\nSettingId:{data.RawData.SettingId}";
        }
    }
}
