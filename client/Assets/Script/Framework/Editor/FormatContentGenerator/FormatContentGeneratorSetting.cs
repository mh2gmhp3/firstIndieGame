using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.FormatContentGenerator
{
    [CreateAssetMenu(
        fileName = "FormatContentGeneratorSetting",
        menuName = "Framework/FormatContentGenerator/Setting")]
    public class FormatContentGeneratorSetting : ScriptableObject
    {
        /// <summary>
        /// 格式類型
        /// </summary>
        private enum FormatType
        {
            /// <summary>字串</summary>
            String,
            /// <summary>檔案</summary>
            File,
        }

        [Serializable]
        private class FormatContentGeneratorSettingData
        {
            /// <summary>格式名稱</summary>
            public string Name;
            /// <summary>格式類型</summary>
            public FormatType Type;
            /// <summary>格式:字串</summary>
            [TextArea]
            public string FormatString;
            /// <summary>格式:檔案</summary>
            public TextAsset FormatFile;

            /// <summary>
            /// 獲取格式結果 結果為行
            /// </summary>
            /// <param name="replaceTextList"></param>
            /// <returns></returns>
            public bool TryGetFormatResultLines(List<ReplaceText> replaceTextList, out List<string> result)
            {
                result = null;

                if (!TryGetFormatResultString(replaceTextList, out var resultString))
                    return false;

                result = resultString.Split('\n').ToList();
                return true;
            }

            /// <summary>
            /// 獲取格式結果 結果為整個字串
            /// </summary>
            /// <param name="replaceTextList"></param>
            /// <returns></returns>
            public bool TryGetFormatResultString(
                List<ReplaceText> replaceTextList,
                out string result)
            {
                result = string.Empty;
                if (replaceTextList == null)
                    return false;

                result = GetFormat();
                for (int i = 0; i < replaceTextList.Count; i++)
                {
                    result = result.Replace(
                        replaceTextList[i].Mark,
                        replaceTextList[i].Text);
                }

                return true;
            }

            /// <summary>
            /// 獲取格式
            /// </summary>
            /// <returns></returns>
            private string GetFormat()
            {
                switch (Type)
                {
                    case FormatType.String:
                        return FormatString;
                    case FormatType.File:
                        return FormatFile == null ? $"{Name} file is null" : FormatFile.text;
                    default:
                        return $"not supported FormatType";
                }

            }
        }

        /// <summary>
        /// 格式列表
        /// </summary>
        [SerializeField]
        private List<FormatContentGeneratorSettingData> _formatSettingList =
            new List<FormatContentGeneratorSettingData>();

        #region Public Method 獲取格式結果

        /// <summary>
        /// 獲取格式結果 結果為行
        /// </summary>
        /// <param name="formatName"></param>
        /// <param name="replaceTextList"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGetFormatResult(
            string formatName,
            List<ReplaceText> replaceTextList,
            out List<string> result)
        {
            result = null;

            if (!TryGetFormatSetting(formatName, out var formatSetting))
                return false;

            return formatSetting.TryGetFormatResultLines(replaceTextList, out result);
        }

        /// <summary>
        /// 獲取格式結果 結果為字串
        /// </summary>
        /// <param name="formatName"></param>
        /// <param name="replaceTextList"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGetFormatResult(
            string formatName,
            List<ReplaceText> replaceTextList,
            out string result)
        {
            result = string.Empty;

            if (!TryGetFormatSetting(formatName, out var formatSetting))
                return false;

            return formatSetting.TryGetFormatResultString(replaceTextList, out result);
        }

        #endregion

        #region Private Method 獲取格式設定

        /// <summary>
        /// 獲取格式設定
        /// </summary>
        /// <param name="formatName"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private bool TryGetFormatSetting(
            string formatName,
            out FormatContentGeneratorSettingData format)
        {
            for (int i = 0; i < _formatSettingList.Count; i++)
            {
                if (_formatSettingList[i].Name.ToLower() == formatName.ToLower())
                {
                    format = _formatSettingList[i];
                    return true;
                }
            }

            format = null;
            return false;
        }

        #endregion
    }
}
