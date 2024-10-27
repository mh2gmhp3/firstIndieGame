using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.ScriptGenerator
{
    public struct TemplateReplaceText
    {
        public string Mark;
        public string Text;
    }

    public class ScriptGenerator
    {
         /// <summary>
        /// 生成腳本
        /// </summary>
        /// <param name="templatePath">範本路徑</param>
        /// <param name="generatedScriptPath">生成的腳本路徑</param>
        /// <param name="templateReplaceTextList">範本中需要替換的內容</param>
        public static void GenScript(
            string templatePath,
            string generatedScriptPath,
            List<TemplateReplaceText> templateReplaceTextList)
        {
            if (!File.Exists(templatePath))
            {
                Debug.LogError("GenScript failed, template file not exist!!");
                return;
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
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }
}
