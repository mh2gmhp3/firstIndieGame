using Extension;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Utility
{
    public class EditorPageContainer
    {
        private List<EditorPage> _pageList = new List<EditorPage>();
        private string[] _pageNames = null;
        private int _curPageIndex = 0;
        private EditorPage _curPage = null;

        private int _gridXCount = 3;

        public EditorPageContainer(int gridXCount)
        {
            _gridXCount = gridXCount;
        }

        public void AddPage(EditorPage page)
        {
            if (page == null)
                return;
            if (IsPageExit(page.Name))
                return;

            page.SetContainer(this);
            _pageList.Add(page);
            _pageNames = _pageList.Select(x => { return x.Name; }).ToArray();
        }

        private bool IsPageExit(string name)
        {
            for (int i = 0; i < _pageList.Count; i++)
            {
                var page = _pageList[i];
                if (page.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            _curPageIndex = GUILayout.SelectionGrid(_curPageIndex, _pageNames, _gridXCount);
            if (EditorGUI.EndChangeCheck())
            {
                ChangeToPage(_curPageIndex);
            }
            if (_curPage != null)
            {
                _curPage.OnGUI();
            }
        }

        public void ChangeToPage(int index)
        {
            if (!_pageList.TryGet(index, out var changePage))
                return;

            if (_curPage != null)
                _curPage.OnDisable();

            _curPageIndex = index;
            _curPage = changePage;
            changePage.OnEnable();
        }

        public void ChangeToPage(string name)
        {
            for (int i = 0; i < _pageList.Count; i++)
            {
                var page = _pageList[i];
                if (page.Name == name)
                {
                    ChangeToPage(i);
                    return;
                }
            }
        }
    }

    public abstract class EditorPage
    {
        private EditorPageContainer _pageContainer;

        public abstract string Name { get; }

        public void SetContainer(EditorPageContainer pageContainer)
        {
            _pageContainer = pageContainer;
        }

        protected void ChangeToPage(string name)
        {
            if (_pageContainer == null)
                return;
            _pageContainer.ChangeToPage(name);
        }

        public virtual void OnEnable() { }
        public virtual void OnGUI() { }
        public virtual void OnDisable() { }
    }
}
