using Framework.Editor.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    public class TerrainEditEnvironmentPage : TerrainEditBasePage
    {
        public override string Name => TerrainEditorDefine.TerrainEditPageToName[(int)TerrainEditPageType.EditEnvironment];

        public TerrainEditEnvironmentPage(TerrainEditorData editorData) : base(editorData)
        {
        }
    }
}
