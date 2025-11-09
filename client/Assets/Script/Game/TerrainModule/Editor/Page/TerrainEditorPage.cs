using Framework.Editor.Utility;
using System.Collections.Generic;

namespace TerrainModule.Editor
{
    public abstract class TerrainEditorPage : EditorPage
    {
        public TerrainEditorPage(TerrainEditorData editorData)
        {
            _editorData = editorData;
        }

        protected TerrainEditorData _editorData;

        protected void ChangeToPage(TerrainEditorPageType page)
        {
            if (!TerrainEditorDefine.PageToName.TryGetValue((int)page, out var name))
                return;
            ChangeToPage(name);
        }
    }
}
