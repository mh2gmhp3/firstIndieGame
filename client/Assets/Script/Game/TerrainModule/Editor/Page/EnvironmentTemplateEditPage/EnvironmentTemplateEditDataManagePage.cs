using Framework.Editor.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class EnvironmentTemplateEditDataPage : EnvironmentTemplateBasePage
    {
        public override string Name => TerrainEditorDefine.EnvironmentTemplateEditPageToName[(int)EnvironmentTemplateEditPageType.Edit];

        public EnvironmentTemplateEditDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }
    }
}
