using DataModule;
using GameMainModule;
using System.Collections.Generic;
using UIModule.Game;
using static UIModule.Game.Window_ItemSelect;
namespace UIModule.Game
{
    public partial class Widget_Page_State : UIWidget
    {
        //#REF#
        private List<Widget_Item> Widget_Item_Weapons;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            Widget_Item_Weapons = _objectReferenceDb.GetObjectList<Widget_Item>("Weapons");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            for(int i = 0; i < Widget_Item_Weapons.Count; i++)
            {
                var index = i;
                Widget_Item_Weapons[index].RegisterWidgetEvent((evData) => {OnWidgetEvent_Widget_Item_Weapons(index, evData);});
            }
            //#INIT_EVENT#
        }

        //#EVENT#
        private void OnWidgetEvent_Widget_Item_Weapons(int index, WidgetEventData eventData)
        {
            var itemList = new List<UIItemData>();
            GameMainSystem.GetItemDataList(itemList, FormModule.TableDefine.ItemType.Weapon);
            UISystem.OpenUIWindow(WindowId.Window_ItemSelect, new UISelectItemDataContainer(itemList,
                (selectedItem) =>
                {
                    GameMainSystem.SetWeapon(index, selectedItem.Id);
                }));
        }

        //#EVENT#
    }
}
