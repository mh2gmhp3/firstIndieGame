using UIModule.Game;
using UIModule;
using UnityEngine;
using DataModule;
namespace UIModule.Game
{
    public partial class Window_ItemSelect : UIWindow
    {
        //#REF#
        private SimpleScrollerController SimpleScrollerController_ItemScroller;
        private Widget_Button Widget_Button_Close;
        private Widget_Item Widget_Item_Select_Item;
        private GameObject GameObject_Item_Info_Root;
        //#REF#

        protected override void InitComponentReference()
        {
            //#INIT_REF#
            SimpleScrollerController_ItemScroller = _objectReferenceDb.GetObject<SimpleScrollerController>("ItemScroller");
            Widget_Button_Close = _objectReferenceDb.GetObject<Widget_Button>("Close");
            Widget_Item_Select_Item = _objectReferenceDb.GetObject<Widget_Item>("Select_Item");
            GameObject_Item_Info_Root = _objectReferenceDb.GetObject<GameObject>("Item_Info_Root");
            //#INIT_REF#
        }

        protected override void InitComponentEvent()
        {
            //#INIT_EVENT#
            SimpleScrollerController_ItemScroller.RegisterScrollerWidgetEvent(OnScrollerWidgetEvent_SimpleScrollerController_ItemScroller);
            Widget_Button_Close.RegisterWidgetEvent(OnWidgetEvent_Widget_Button_Close);
            Widget_Item_Select_Item.RegisterWidgetEvent(OnWidgetEvent_Widget_Item_Select_Item);
            //#INIT_EVENT#
        }

        //#EVENT#
        private void OnScrollerWidgetEvent_SimpleScrollerController_ItemScroller(WidgetEventData eventData)
        {
            if (eventData.UIData is UIItemData itemData)
            {
                GameObject_Item_Info_Root.SetActive(true);
                Widget_Item_Select_Item.SetData(itemData);
            }
        }

        private void OnWidgetEvent_Widget_Button_Close(WidgetEventData eventData)
        {
            SetVisible(false);
        }

        private void OnWidgetEvent_Widget_Item_Select_Item(WidgetEventData eventData)
        {

        }

        //#EVENT#
    }
}
