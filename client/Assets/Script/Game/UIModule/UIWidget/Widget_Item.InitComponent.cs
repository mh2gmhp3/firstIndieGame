using UnityEngine.UI;
namespace UIModule.Game
{
    public partial class Widget_Item : UIWidget
    {
        //#REF#
        private Image Image_Background;
        private Text Text_Content;
        private Button Button_Trigger;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            Image_Background = _objectReferenceDb.GetObject<Image>("Background");
            Text_Content = _objectReferenceDb.GetObject<Text>("Content");
            Button_Trigger = _objectReferenceDb.GetObject<Button>("Trigger");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            Button_Trigger.onClick.AddListener(OnClick_Button_Trigger);
            //#INIT_EVENT#
        }

        //#EVENT#
        private void OnClick_Button_Trigger()
        {
            InvokeWidgetEvent(new WidgetEventData() { UIData = _uiData });
        }

        //#EVENT#
    }
}
