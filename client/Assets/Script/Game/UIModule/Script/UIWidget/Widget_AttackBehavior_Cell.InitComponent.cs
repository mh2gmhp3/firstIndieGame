using UnityEngine.UI;
using UIModule.Game;
namespace UIModule.Game
{
    public partial class Widget_AttackBehavior_Cell : UIWidget
    {
        //#REF#
        private Text Text_Content;
        private Widget_Button Widget_Button_Button;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            Text_Content = _objectReferenceDb.GetObject<Text>("Content");
            Widget_Button_Button = _objectReferenceDb.GetObject<Widget_Button>("Button");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            Widget_Button_Button.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_Button);
            //#INIT_EVENT#
        }

        //#EVENT#
        private void OnWidgetEvent_Widget_Button_Button(WidgetEventData eventData)
        {
            InvokeWidgetEvent(new WidgetEventData() { UIData = _uiData });
        }

        //#EVENT#
    }
}
