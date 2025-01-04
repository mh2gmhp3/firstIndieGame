using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.FormatContentGenerator
{
    public static class FormatContentGenerator
    {
        #region Public Static Method

        /// <summary>
        /// 獲取格式結果 結果為行
        /// </summary>
        /// <param name="formatSettingPath"></param>
        /// <param name="formatName"></param>
        /// <param name="replaceTextList"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryGetFormatResult(
            string formatSettingPath,
            string formatName,
            List<ReplaceText> replaceTextList,
            out List<string> result)
        {
            result = null;

            if (!TryGetInstance(formatSettingPath, out var formatSetting))
                return false;

            return formatSetting.TryGetFormatResult(formatName, replaceTextList, out result);
        }

        /// <summary>
        /// 獲取格式結果 結果為字串
        /// </summary>
        /// <param name="formatSettingPath"></param>
        /// <param name="formatName"></param>
        /// <param name="replaceTextList"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryGetFormatResult(
            string formatSettingPath,
            string formatName,
            List<ReplaceText> replaceTextList,
            out string result)
        {
            result = string.Empty;

            if (!TryGetInstance(formatSettingPath, out var formatSetting))
                return false;

            return formatSetting.TryGetFormatResult(formatName, replaceTextList, out result);
        }

        #endregion

        #region Private Static Method

        /// <summary>
        /// 路徑對設定檔實體暫存
        /// </summary>
        private static Dictionary<string, FormatContentGeneratorSetting> _pathToInstanceCache =
            new Dictionary<string, FormatContentGeneratorSetting>();

        /// <summary>
        /// 獲取格式設定檔
        /// </summary>
        /// <param name="formatSettingPath"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static bool TryGetInstance(
            string formatSettingPath,
            out FormatContentGeneratorSetting format)
        {
            if (_pathToInstanceCache.TryGetValue(formatSettingPath, out format))
                return true;

            format = AssetDatabase.LoadAssetAtPath<FormatContentGeneratorSetting>(formatSettingPath);
            if (format == null)
                return false;

            return true;
        }

        #endregion
    }
}
