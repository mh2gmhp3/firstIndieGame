using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class BlockTemplateCreateDataPage : BlockTemplateEditBasePage
    {
        public override string Name => TerrainEditorDefine.BlockTemplateEditPageToName[(int)BlockTemplateEditPageType.Create];

        public BlockTemplateCreateDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }
    }
}
