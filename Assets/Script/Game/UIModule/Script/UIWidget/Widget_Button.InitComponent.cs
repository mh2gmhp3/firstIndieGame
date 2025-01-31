using UnityEngine.UI;
namespace UIModule.Game
{
    public partial class Widget_Button : UIWidget
    {
        //#REF#
        private Button Button_Obj;
        private Text Text_Content;
        //#REF#

        protected override void InitComponentRefreence()
        {
            //#INIT_REF#
            Button_Obj = _objectReferenceDb.GetObject<Button>("Obj");
            Text_Content = _objectReferenceDb.GetObject<Text>("Content");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            Button_Obj.onClick.AddListener(OnClick_Button_Obj);
            //#INIT_EVENT#
        }

        //#EVENT#
        private void OnClick_Button_Obj()
        {
            InvokeWidgetEvent(default);
        }

        //#EVENT#
    }
}
