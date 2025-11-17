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
        DataManage,
        Edit,
    }

    public enum BlockTemplateEditPageType
    {
        DataManage,
        Edit,
    }

    public class TerrainEditorDefine
    {
        public const string EditorScenePath = "Assets/Script/Game/TerrainModule/Editor/TerrainEditor.unity";
        public const string EditDataFolderPath = "Assets/Script/Game/TerrainModule/Editor/EditData";
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
                { (int)TerrainEditPageType.DataManage, "檔案管理"},
                { (int)TerrainEditPageType.Edit, "編輯"}
            };

        public static Dictionary<int, string> BlockTemplateEditPageToName =
            new Dictionary<int, string>()
            {
                { (int)BlockTemplateEditPageType.DataManage, "檔案管理"},
                { (int)BlockTemplateEditPageType.Edit, "編輯"}
            };
    }
}
