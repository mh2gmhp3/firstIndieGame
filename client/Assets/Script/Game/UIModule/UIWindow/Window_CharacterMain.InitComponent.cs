using GameMainModule;
using System.Collections.Generic;
using UIModule.Game;
using UnityEngine;
namespace UIModule.Game
{
    public partial class Window_CharacterMain : UIWindow
    {
        //#REF#
        private Widget_Button Widget_Button_Close;
        private List<Widget_Button> Widget_Button_Page_State;
        private Widget_Page_State Widget_Page_State_Obj;
        private Widget_Page_WeaponEdit Widget_Page_WeaponEdit_Obj;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            Widget_Button_Close = _objectReferenceDb.GetObject<Widget_Button>("Close");
            Widget_Button_Page_State = _objectReferenceDb.GetObjectList<Widget_Button>("Page_State");
            Widget_Page_State_Obj = _objectReferenceDb.GetObject<Widget_Page_State>("Obj");
            Widget_Page_WeaponEdit_Obj = _objectReferenceDb.GetObject<Widget_Page_WeaponEdit>("Obj");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            Widget_Button_Close.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_Close);
            for(int i = 0; i < Widget_Button_Page_State.Count; i++)
            {
                var index = i;
                Widget_Button_Page_State[index].RegisterWidgetEvent((evData) => {OnWidgetEvent_Widget_Button_Page_State(index, evData);});
            }
            Widget_Page_State_Obj.RegisterWidgetEvent(OnWidgetEvent_Widget_Page_State_Obj);
            Widget_Page_WeaponEdit_Obj.RegisterWidgetEvent(OnWidgetEvent_Widget_Page_WeaponEdit_Obj);
            //#INIT_EVENT#
        }

        //#EVENT#
        private void OnWidgetEvent_Widget_Button_Close(WidgetEventData eventData)
        {
            GameMainSystem.CloseCharacterMain();
        }

        private void OnWidgetEvent_Widget_Button_Page_State(int index, WidgetEventData eventData)
        {
            SetPage((Page)index);
        }

        private void OnWidgetEvent_Widget_Page_State_Obj(WidgetEventData eventData)
        {

        }

        private void OnWidgetEvent_Widget_Page_WeaponEdit_Obj(WidgetEventData eventData)
        {

        }

        //#EVENT#
    }
}
