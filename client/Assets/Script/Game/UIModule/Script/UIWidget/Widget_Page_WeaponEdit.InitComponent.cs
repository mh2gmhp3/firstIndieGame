using System.Collections.Generic;
using UIModule.Game;
using UIModule;
using UnityEngine;
namespace UIModule.Game
{
    public partial class Widget_Page_WeaponEdit : UIWidget
    {
        //#REF#
        private List<Widget_Item> Widget_Item_Weapons;
        private SimpleScrollerController SimpleScrollerController_AttackBehavior;
        private GameObject GameObject_AttackBehavior_Root;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            Widget_Item_Weapons = _objectReferenceDb.GetObjectList<Widget_Item>("Weapons");
            SimpleScrollerController_AttackBehavior = _objectReferenceDb.GetObject<SimpleScrollerController>("AttackBehavior");
            GameObject_AttackBehavior_Root = _objectReferenceDb.GetObject<GameObject>("AttackBehavior_Root");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            for(int i = 0; i < Widget_Item_Weapons.Count; i++)
            {
            	var index = i;
            	Widget_Item_Weapons[index].RegisterWidgetEvent((evData) => {OnWidgetEvent_Widget_Item_Weapons(index, evData);});
            }
            SimpleScrollerController_AttackBehavior.RegisterScrollerWidgetEvent(OnScrollerWidgetEvent_SimpleScrollerController_AttackBehavior);
            //#INIT_EVENT#
        }

        //#EVENT#
        private void OnWidgetEvent_Widget_Item_Weapons(int index, WidgetEventData eventData)
        {
        
        }
        
        private void OnScrollerWidgetEvent_SimpleScrollerController_AttackBehavior(WidgetEventData eventData)
        {
        
        }
        
        //#EVENT#
    }
}
