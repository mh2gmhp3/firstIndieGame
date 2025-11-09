namespace TerrainModule.Editor
{
    public class EditDataPage : TerrainEditorPage
    {
        public EditDataPage(TerrainEditorData editorData) : base(editorData)
        {
        }

        public override string Name => TerrainEditorDefine.PageToName[(int)TerrainEditorPageType.Edit];

        public override void OnGUI()
        {

        }
    }
}
