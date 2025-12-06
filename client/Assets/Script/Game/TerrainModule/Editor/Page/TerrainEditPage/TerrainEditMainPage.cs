using Framework.Editor.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    public abstract class TerrainEditBasePage : EditorPage
    {
        public TerrainEditBasePage(TerrainEditorData editorData)
        {
            _editorData = editorData;
        }

        protected TerrainEditorData _editorData;

        protected void ChangeToPage(TerrainEditPageType page)
        {
            if (!TerrainEditorDefine.TerrainEditPageToName.TryGetValue((int)page, out var name))
                return;
            ChangeToPage(name);
        }
    }

    public class TerrainEditMainPage : TerrainEditorBasePage
    {
        public override string Name => TerrainEditorDefine.TerrainEditorPageToName[(int)TerrainEditorPageType.TerrainEdit];

        private EditorPageContainer _pageContainer;

        public TerrainEditMainPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        protected override void OnAddToContainer()
        {
            _pageContainer = new EditorPageContainer(4, GetPageContainerRepaintEvent());
            _pageContainer.AddPage(new TerrainDataManagePage(_editorData));
            _pageContainer.AddPage(new TerrainEditDataPage(_editorData));
            _pageContainer.AddPage(new TerrainEditEnvironmentPage(_editorData));
            _pageContainer.AddPage(new TerrainEditAreaPage(_editorData));
            _pageContainer.ChangeToPage(0);
        }

        public override void OnEnable()
        {
            _editorData.TerrainEditorMgr.ChangeMode(TerrainEditorManager.EditorMode.Terrain);
        }

        public override void OnGUI()
        {
            _pageContainer.OnGUI();
        }

        public override void OnSceneGUI()
        {
            _pageContainer.OnSceneGUI();
        }
    }
}
