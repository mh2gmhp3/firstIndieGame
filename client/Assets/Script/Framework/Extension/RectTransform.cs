using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Extension
{
    public static class RectTransformExtension
    {
        /// <summary>
        /// 設定貼邊延展
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="anchoredPosition"></param>
        /// <param name="sizeDelta"></param>
        public static void SetStretch(
            this RectTransform rectTransform,
            Vector2 anchoredPosition = default,
            Vector2 sizeDelta = default)
        {
            if (rectTransform == null)
                return;

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.anchoredPosition = anchoredPosition;
        }
    }
}
