using UnityEngine;
using UIModule.Game;
using UIModule;
using Logging;
namespace UIModule.Game
{
    public partial class Window_AttackBehaviorEdit : UIWindow
    {
        //#REF#
        private Widget_Button Widget_Button_Close;
        private SimpleScrollerController SimpleScrollerController_AttackBehavior;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            Widget_Button_Close = _objectReferenceDb.GetObject<Widget_Button>("Close");
            SimpleScrollerController_AttackBehavior = _objectReferenceDb.GetObject<SimpleScrollerController>("AttackBehavior");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            Widget_Button_Close.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_Close);
            SimpleScrollerController_AttackBehavior.RegisterScrollerWidgetEvent(OnScrollerWidgetEvent_SimpleScrollerController_AttackBehavior);
            //#INIT_EVENT#
        }

        //#EVENT#
        private void OnWidgetEvent_Widget_Button_Close(WidgetEventData eventData)
        {
            SetVisible(false);
        }

        private void OnScrollerWidgetEvent_SimpleScrollerController_AttackBehavior(WidgetEventData eventData)
        {
            if (eventData.UIData is UIAttackBehaviorData data)
            {
                //Log.LogInfo($"Click:{data.RawData.Id}");
            }
        }

        //#EVENT#
    }
}
