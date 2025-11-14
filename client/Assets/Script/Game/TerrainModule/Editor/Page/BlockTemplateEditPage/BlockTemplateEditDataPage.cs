using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TerrainModule.Editor.TerrainEditorManager;

namespace TerrainModule.Editor
{
    public class BlockTemplateEditDataPage : BlockTemplateEditBasePage
    {
        public override string Name => TerrainEditorDefine.BlockTemplateEditPageToName[(int)BlockTemplateEditPageType.Edit];

        private BlockTemplatePreviewSetting _previewSetting = new BlockTemplatePreviewSetting();

        public BlockTemplateEditDataPage(TerrainEditorData editorData) : base(editorData)
        {
            editorData.BlockTemplatePreviewSetting = _previewSetting;
        }
    }
}
