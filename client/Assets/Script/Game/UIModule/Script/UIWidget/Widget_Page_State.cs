using DataModule;
using GameMainModule;
using static UIModule.Game.Widget_Item;
using System.Collections.Generic;

namespace UIModule.Game
{
    public partial class Widget_Page_State : UIWidget
    {
        private EmptyData _emptyData = new EmptyData();

        protected override void DoSetData()
        {
            OnUIDataNotify(default);
        }

        protected override void OnUIDataNotify(IUIDataNotifyInfo notifyInfo)
        {
            if (_uiData is UICharacterData characterData)
            {
                SetWeapon(characterData.WeaponRefItemIdList);
            }
        }

        private void SetWeapon(List<int> weaponRefItemIdList)
        {
            var weaponCount = weaponRefItemIdList.Count;
            for (int i = 0; i < Widget_Item_Weapons.Count; i++)
            {
                var widget = Widget_Item_Weapons[i];
                if (i >= weaponCount)
                {
                    widget.SetData(_emptyData);
                    continue;
                }

                var itemId = weaponRefItemIdList[i];
                if (!GameMainSystem.TryGetItemData(itemId, out var itemData))
                {
                    widget.SetData(_emptyData);
                    continue;
                }

                widget.SetData(itemData);
            }
        }
    }
}
