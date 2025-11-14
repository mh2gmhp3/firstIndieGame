using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    public enum TerrainEditorPageType
    {
        TerrainEdit,
        BlockTemplateEdit,
    }

    public enum TerrainEditPageType
    {
        Create,
        Load,
        Edit,
    }

    public enum BlockTemplateEditPageType
    {
        Create,
        Load,
        Edit,
    }

    public class TerrainEditorDefine
    {
        public const string EditorScenePath = "Assets/Script/Game/TerrainModule/Editor/TerrainEditor.unity";
        public const string EditDataFolderPath = "Assets/Script/Game/TerrainModule/EditData";
        public const string EditTerrainDataFolderPath = EditDataFolderPath + "/TerrainData";
        public const string EditBlockTemplateDataFolderPath = EditDataFolderPath + "/BlockTemplateData";

        public const string ManagerName = "TerrainEditorManager";

        public const string Dialog_Title_Error = "錯誤";
        public const string Dialog_Ok_Confirm = "確認";

        public static Dictionary<int, string> TerrainEditorPageToName =
            new Dictionary<int, string>()
            {
                { (int)TerrainEditorPageType.TerrainEdit, "地形"},
                { (int)TerrainEditorPageType.BlockTemplateEdit, "方格範本"}
            };

        public static Dictionary<int, string> TerrainEditPageToName =
            new Dictionary<int, string>()
            {
                { (int)TerrainEditPageType.Create, "建立"},
                { (int)TerrainEditPageType.Load, "讀取"},
                { (int)TerrainEditPageType.Edit, "編輯"}
            };

        public static Dictionary<int, string> BlockTemplateEditPageToName =
            new Dictionary<int, string>()
            {
                { (int)BlockTemplateEditPageType.Create, "建立"},
                { (int)BlockTemplateEditPageType.Edit, "編輯"}
            };
    }
}
