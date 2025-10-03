using GameMainModule;
using UIModule.Game;
using static UIModule.Game.Window_AttackBehaviorEdit;
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
            UISystem.OpenUIWindow(
                "Window_AttackBehaviorEdit",
                new UIAttackBehaviorDataContainer(GameMainSystem.GetAttackBhaviorDataList()));
        }

        //#EVENT#
    }
}
