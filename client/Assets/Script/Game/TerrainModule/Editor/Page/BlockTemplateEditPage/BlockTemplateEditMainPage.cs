using Framework.Editor.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    public abstract class BlockTemplateEditBasePage : EditorPage
    {
        protected TerrainEditorData _editorData;

        public BlockTemplateEditBasePage(TerrainEditorData editorData)
        {
            _editorData = editorData;
        }

        protected void ChangeToPage(BlockTemplateEditPageType page)
        {
            if (!TerrainEditorDefine.BlockTemplateEditPageToName.TryGetValue((int)page, out var name))
                return;
            ChangeToPage(name);
        }
    }

    public class BlockTemplateEditMainPage : TerrainEditorBasePage
    {
        public override string Name => TerrainEditorDefine.TerrainEditorPageToName[(int)TerrainEditorPageType.BlockTemplateEdit];

        private EditorPageContainer _pageContainer;

        public BlockTemplateEditMainPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        protected override void OnAddToContainer()
        {
            _pageContainer = new EditorPageContainer(2, GetPageContainerRepaintEvent());
            _pageContainer.AddPage(new BlockTemplateDataManagePage(_editorData));
            _pageContainer.AddPage(new BlockTemplateEditDataPage(_editorData));
            _pageContainer.ChangeToPage(0);
        }

        public override void OnEnable()
        {
            _editorData.TerrainEditorMgr.ChangeMode(TerrainEditorManager.EditorMode.BlockTemplate);
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
