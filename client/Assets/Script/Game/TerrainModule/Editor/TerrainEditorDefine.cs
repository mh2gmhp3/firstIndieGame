using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainModule.Editor
{
    public enum TerrainEditorPageType
    {
        Create,
        Load,
        Edit,
    }

    public class TerrainEditorDefine
    {
        public const string EditorScenePath = "Assets/Script/Game/TerrainModule/Editor/TerrainEditor.unity";
        public const string EditDataFolderPath = "Assets/Script/Game/TerrainModule/EditData";

        public const string ManagerName = "TerrainEditorManager";

        public const string Dialog_Title_Error = "錯誤";
        public const string Dialog_Ok_Confirm = "確認";

        public static Dictionary<int, string> PageToName =
            new Dictionary<int, string>()
            {
                { (int)TerrainEditorPageType.Create, "建立新檔"},
                { (int)TerrainEditorPageType.Load, "讀取檔案"},
                { (int)TerrainEditorPageType.Edit, "編輯"}
            };
    }
}
