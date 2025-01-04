using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.ScriptGenerator
{
    public class ScriptGenerator
    {
         /// <summary>
        /// 生成腳本
        /// </summary>
        /// <param name="templatePath">範本路徑</param>
        /// <param name="generatedScriptPath">生成的腳本路徑</param>
        /// <param name="templateReplaceTextList">範本中需要替換的內容</param>
        public static Object GenScript(
            string templatePath,
            string generatedScriptPath,
            List<ReplaceText> templateReplaceTextList)
        {
            if (!File.Exists(templatePath))
            {
                Debug.LogError("GenScript failed, template file not exist!!");
                return null;
            }

            var folderPath = Path.GetDirectoryName(generatedScriptPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var scriptText = File.ReadAllText(templatePath);
            for (int i = 0; i < templateReplaceTextList.Count; i++)
            {
                scriptText = scriptText.Replace(
                    templateReplaceTextList[i].Mark,
                    templateReplaceTextList[i].Text);
            }

            File.WriteAllText(generatedScriptPath, scriptText, Encoding.UTF8);
            AssetDatabase.ImportAsset(generatedScriptPath);
            return AssetDatabase.LoadAssetAtPath(generatedScriptPath, typeof(Object));
        }
    }
}
