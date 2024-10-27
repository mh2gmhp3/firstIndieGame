using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Utility
{
    /// <summary>
    /// 可擴展文字區域
    /// </summary>
    public class ExpandTextField
    {
        private bool _expand;
        private string _text;

        public string Text => _text;

        /// <summary>
        /// 觸發擴展
        /// </summary>
        public void TriggerExpand()
        {
            _expand = !_expand;
        }

        /// <summary>
        /// 重設
        /// </summary>
        public void Reset()
        {
            _expand = false;
            _text = string.Empty;
        }

        /// <summary>
        /// 繪製文字區域
        /// </summary>
        /// <returns></returns>
        public void DrawTextFieldIfExpand()
        {
            if (!_expand)
                return;

            _text = EditorGUILayout.TextField(_text);
        }

        /// <summary>
        /// 繪製按鈕
        /// </summary>
        /// <returns></returns>
        public bool DrawButtonIfExpand(string text)
        {
            if (!_expand)
                return false;

            return GUILayout.Button(text);
        }
    }
}
