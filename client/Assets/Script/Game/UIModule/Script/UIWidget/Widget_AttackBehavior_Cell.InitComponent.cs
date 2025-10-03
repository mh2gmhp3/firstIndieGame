using UnityEngine.UI;
namespace UIModule.Game
{
    public partial class Widget_AttackBehavior_Cell : UIWidget
    {
        //#REF#
        private Text Text_Content;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            Text_Content = _objectReferenceDb.GetObject<Text>("Content");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            //#INIT_EVENT#
        }

        //#EVENT#
        //#EVENT#
    }
}
