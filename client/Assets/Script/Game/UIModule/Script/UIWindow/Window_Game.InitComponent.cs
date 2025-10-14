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
        private Widget_Button Widget_Button_CharacterMain;
        private Widget_Button Widget_Button_Save_Test;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            Widget_Button_CharacterMain = _objectReferenceDb.GetObject<Widget_Button>("CharacterMain");
            Widget_Button_Save_Test = _objectReferenceDb.GetObject<Widget_Button>("Save_Test");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            Widget_Button_CharacterMain.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_CharacterMain);
            Widget_Button_Save_Test.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_Save_Test);
            //#INIT_EVENT#
        }

        //#EVENT#

        private void OnWidgetEvent_Widget_Button_CharacterMain(WidgetEventData eventData)
        {
            UISystem.OpenUIWindow(WindowId.Window_CharacterMain, GameMainSystem.GetUICharacterData());
        }

        private void OnWidgetEvent_Widget_Button_Save_Test(WidgetEventData eventData)
        {
            GameMainSystem.SaveCurData();
        }

        //#EVENT#
    }
}
