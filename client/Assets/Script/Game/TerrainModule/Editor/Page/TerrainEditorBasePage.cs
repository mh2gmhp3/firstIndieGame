using Framework.Editor.Utility;

namespace TerrainModule.Editor
{
    public abstract class TerrainEditorBasePage : EditorPage
    {
        public TerrainEditorBasePage(TerrainEditorData editorData)
        {
            _editorData = editorData;
        }

        protected TerrainEditorData _editorData;

        protected void ChangeToPage(TerrainEditorPageType page)
        {
            if (!TerrainEditorDefine.TerrainEditorPageToName.TryGetValue((int)page, out var name))
                return;
            ChangeToPage(name);
        }
    }
}
