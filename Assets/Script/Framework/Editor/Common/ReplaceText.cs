using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Editor
{
    /// <summary>
    /// 取代文字用結構
    /// </summary>
    public struct ReplaceText
    {
        /// <summary>
        /// 被取代的標記
        /// </summary>
        public string Mark;
        /// <summary>
        /// 取代後的內容
        /// </summary>
        public string Text;

        public ReplaceText(string mark, string text)
        {
            Mark = mark;
            Text = text;
        }
    }
}
