using UnityEngine;
using UIModule.Game;
namespace UIModule.Game
{
    public partial class Window_AttackBehaviorEdit : UIWindow
    {
        //#REF#
        private GameObject GameObject_AttackBehavior_Cell_Template;
        private RectTransform RectTransform_Scroller_Content;
        private Widget_Button Widget_Button_Close;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            GameObject_AttackBehavior_Cell_Template = _objectReferenceDb.GetObject<GameObject>("AttackBehavior_Cell_Template");
            RectTransform_Scroller_Content = _objectReferenceDb.GetObject<RectTransform>("Scroller_Content");
            Widget_Button_Close = _objectReferenceDb.GetObject<Widget_Button>("Close");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            Widget_Button_Close.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_Close);
            //#INIT_EVENT#
        }

        //#EVENT#
        private void OnWidgetEvent_Widget_Button_Close(WidgetEventData eventData)
        {
            SetVisible(false);
        }

        //#EVENT#
    }
}
