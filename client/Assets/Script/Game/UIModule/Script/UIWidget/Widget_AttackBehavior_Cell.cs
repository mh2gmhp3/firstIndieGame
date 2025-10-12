using FormModule;
using GameMainModule;
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
            if (FormSystem.Table.AttackBehaviorSettingTable.TryGetData(data.RawData.SettingId, out var row) &&
                GameMainSystem.AttackBehaviorAssetSetting.TryGetSetting(row.AssetSettingId, out var assetSetting))
            {
                Text_Content.text =
                    $"Id:{data.RawData.RefItemId}\n" +
                    $"SettingId:{data.RawData.SettingId}\n" +
                    $"AssetSettingId:{row.AssetSettingId}\n" +
                    $"AreaType:{assetSetting.CollisionAreaType}";
            }

        }
    }
}
