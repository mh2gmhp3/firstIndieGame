using UnityEngine.UI;
using System.Collections.Generic;
using UIModule.Game;
using Logging;
using System;
using GameMainModule;

namespace UIModule.Game
{
    public partial class Window_Main : UIWindow
    {
        //#REF#
        private Widget_Button Widget_Button_StartNew;
        private Widget_Button Widget_Button_Continue;
        private Widget_Button Widget_Button_Load;
        private Widget_Button Widget_Button_Setting;
        private Widget_Button Widget_Button_Info;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            Widget_Button_StartNew = _objectReferenceDb.GetObject<Widget_Button>("StartNew");
            Widget_Button_Continue = _objectReferenceDb.GetObject<Widget_Button>("Continue");
            Widget_Button_Load = _objectReferenceDb.GetObject<Widget_Button>("Load");
            Widget_Button_Setting = _objectReferenceDb.GetObject<Widget_Button>("Setting");
            Widget_Button_Info = _objectReferenceDb.GetObject<Widget_Button>("Info");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            Widget_Button_StartNew.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_StartNew);
            Widget_Button_Continue.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_Continue);
            Widget_Button_Load.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_Load);
            Widget_Button_Setting.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_Setting);
            Widget_Button_Info.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_Info);
            //#INIT_EVENT#
        }

        //#EVENT#
        private void OnWidgetEvent_Widget_Button_StartNew(WidgetEventData eventData)
        {
            GameMainSystem.StartNewGame();
            SetVisible(false);
        }

        private void OnWidgetEvent_Widget_Button_Continue(WidgetEventData eventData)
        {
            GameMainSystem.ContinueGame();
        }

        private void OnWidgetEvent_Widget_Button_Load(WidgetEventData eventData)
        {
            GameMainSystem.LoadGame();
        }

        private void OnWidgetEvent_Widget_Button_Setting(WidgetEventData eventData)
        {
            GameMainSystem.Setting();
        }

        private void OnWidgetEvent_Widget_Button_Info(WidgetEventData eventData)
        {
            GameMainSystem.Info();
        }

        //#EVENT#
    }
}
