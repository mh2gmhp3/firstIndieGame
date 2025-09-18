using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UIModule
{
    public static class UIEditorDefine
    {
        public static string UI_SCRIPT_TAMPLATE_FILE_MAIN_PATH = "Assets/Script/Framework/UIModule/Editor/ScriptTemplate";
        public static string UI_SCRIPT_TAMPLATE_FILE_NAME_FORMAT = "{0}Template";
        public static string UI_SCRIPT_INIT_COMPONENT_FILE_NAME = "InitComponent";
        public static string UI_SCRIPT_FOLDER_PATH = "Assets/Script/Game/UIModule/Script";

        /// <summary>
        /// 獲取UI腳本資料夾路徑
        /// </summary>
        /// <param name="uiTypeName">UI類型名稱</param>
        /// <returns></returns>
        public static string GetUIScriptFolderPath(string uiTypeName)
        {
            return Path.Combine(UI_SCRIPT_FOLDER_PATH, uiTypeName);
        }

        /// <summary>
        /// 獲取UI腳本路徑
        /// </summary>
        /// <param name="uiTypeName">UI類型名稱</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetUIScriptPath(string uiTypeName, string componentName)
        {
            var folderPath = GetUIScriptFolderPath(uiTypeName);
            return Path.Combine(folderPath, $"{componentName}.cs");
        }

        /// <summary>
        /// 獲取UI初始化元件腳本路徑
        /// </summary>
        /// <param name="uiTypeName">UI類型名稱</param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetUIScriptInitComponentPath(string uiTypeName, string componentName)
        {
            var folderPath = GetUIScriptFolderPath(uiTypeName);
            return Path.Combine(folderPath, $"{componentName}.{UI_SCRIPT_INIT_COMPONENT_FILE_NAME}.cs");
        }
    }
}