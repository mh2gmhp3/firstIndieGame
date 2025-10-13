using DataModule;
using GameMainModule;
using System.Collections.Generic;
using UIModule.Game;
using static UIModule.Game.Window_AttackBehaviorEdit;
using static UIModule.Game.Window_ItemSelect;
namespace UIModule.Game
{
    public partial class Window_Game : UIWindow
    {
        //#REF#
        private Widget_Button Widget_Button_AttackBehavior;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            Widget_Button_AttackBehavior = _objectReferenceDb.GetObject<Widget_Button>("AttackBehavior");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            Widget_Button_AttackBehavior.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_AttackBehavior);
            //#INIT_EVENT#
        }

        //#EVENT#
        private void OnWidgetEvent_Widget_Button_AttackBehavior(WidgetEventData eventData)
        {
            //UISystem.OpenUIWindow(
            //    "Window_AttackBehaviorEdit",
            //    new UIAttackBehaviorDataContainer(GameMainSystem.GetAttackBehaviorDataList()));

            //var itemList = new List<UIItemData>();
            //GameMainSystem.GetItemDataList(itemList, FormModule.TableDefine.ItemType.Weapon);
            //UISystem.OpenUIWindow(
            //    "Window_ItemSelect",
            //    new UISelectItemDataContainer(itemList));


            UISystem.OpenUIWindow("Window_CharacterMain", null);
        }

        //#EVENT#
    }
}
