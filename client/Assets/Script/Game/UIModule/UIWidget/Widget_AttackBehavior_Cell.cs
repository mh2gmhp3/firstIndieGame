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
            if (_uiData is UIAttackBehaviorEditData editData)
            {
                if (editData.RefItemId == CommonDefine.EmptyWeaponId)
                {
                    Text_Content.text = "Empty";
                }
                else if (GameMainSystem.TryGetAttackBehaviorData(editData.RefItemId, out var attackBehaviorData))
                {
                    if (FormSystem.Table.AttackBehaviorSettingTable.TryGetData(attackBehaviorData.SettingId, out var row) &&
                        GameMainSystem.AttackBehaviorAssetSetting.TryGetSetting(row.AssetSettingId, out var assetSetting))
                    {
                        Text_Content.text =
                            $"Id:{attackBehaviorData.RefItemId}\n" +
                            $"SettingId:{attackBehaviorData.SettingId}\n" +
                            $"AssetSettingId:{row.AssetSettingId}\n" +
                            $"AreaType:{assetSetting.CollisionAreaType}";
                    }
                    else
                    {
                        Text_Content.text = $"Data not Found, " +
                            $"ItemId:{editData.RefItemId}, " +
                            $"SettingId:{attackBehaviorData.SettingId}";
                    }
                }
            }
            else if (_uiData is UIAttackBehaviorData data)
            {
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
}
