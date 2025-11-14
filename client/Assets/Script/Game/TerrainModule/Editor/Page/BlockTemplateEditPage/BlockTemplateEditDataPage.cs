using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class BlockTemplateEditDataPage : BlockTemplateEditBasePage
    {
        public override string Name => TerrainEditorDefine.BlockTemplateEditPageToName[(int)BlockTemplateEditPageType.Edit];

        public BlockTemplateEditDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }
    }
}
