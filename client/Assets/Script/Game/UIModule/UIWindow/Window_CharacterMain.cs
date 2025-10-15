using DataModule;
using Logging;
using System.Collections.Generic;
using UnityEngine;

namespace UIModule.Game
{
    public partial class Window_CharacterMain : UIWindow
    {
        private enum Page
        {
            State      = 0,
            WeaponEdit = 1,
        }

        private Page _curPage = Page.State;
        private Dictionary<int, UIWidget> _pageToWidgetDic = new Dictionary<int, UIWidget>();

        private CharacterData _characterData;

        protected override void OnInit()
        {
            _pageToWidgetDic.Add((int)Page.State, Widget_Page_State_Obj);
            _pageToWidgetDic.Add((int)Page.WeaponEdit, Widget_Page_WeaponEdit_Obj);
        }

        protected override void DoOpen(IUIData uiData)
        {
            if (_uiData is CharacterData characterData)
            {
                _characterData = characterData;
                SetPage(Page.State, true);
            }
            else
            {
                Log.LogError("Window_CharacterMain DoOpen Error, dat can not handle");
                SetVisible(false);
            }
        }

        protected override void DoClose()
        {

        }

        protected override void DoNotify(IUIData data, IUIDataNotifyInfo notifyInfo)
        {

        }

        private void SetPage(Page page, bool force = false)
        {
            if (_curPage == page && !force)
                return;

            if (!_pageToWidgetDic.TryGetValue((int)page, out var pageWidget))
            {
                Log.LogError($"Window_CharacterMain SetPage Error, Page not found Page:{page}");
                return;
            }

            _curPage = page;
            foreach (var widget in _pageToWidgetDic.Values)
            {
                widget.SetVisible(false);
            }

            pageWidget.SetVisible(true);
            pageWidget.SetData(_characterData);
        }
    }
}
