using Framework.Editor.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    public abstract class EnvironmentTemplateBasePage : EditorPage
    {
        public EnvironmentTemplateBasePage(TerrainEditorData editorData)
        {
            _editorData = editorData;
        }

        protected TerrainEditorData _editorData;

        protected void ChangeToPage(EnvironmentTemplateEditPageType page)
        {
            if (!TerrainEditorDefine.EnvironmentTemplateEditPageToName.TryGetValue((int)page, out var name))
                return;
            ChangeToPage(name);
        }
    }

    public class EnvironmentTemplateMainPage : TerrainEditorBasePage
    {
        public override string Name => TerrainEditorDefine.TerrainEditorPageToName[(int)TerrainEditorPageType.EnvironmentTemplateEdit];

        private EditorPageContainer _pageContainer;

        public EnvironmentTemplateMainPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        protected override void OnAddToContainer()
        {
            _pageContainer = new EditorPageContainer(2, GetPageContainerRepaintEvent());
            _pageContainer.AddPage(new EnvironmentTemplateDataManagePage(_editorData));
            _pageContainer.AddPage(new EnvironmentTemplateEditDataPage(_editorData));
            _pageContainer.ChangeToPage(0);
        }

        public override void OnEnable()
        {
            _editorData.TerrainEditorMgr.ChangeMode(TerrainEditorManager.EditorMode.EnvironmentTemplate);
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
