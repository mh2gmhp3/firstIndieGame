using System.Reflection;
using UnityEditorInternal;
using UnityEngine;

namespace Framework.Editor.Utility
{
    public class UnityEditorInternalExtension
    {
        /// <summary>
        /// 添加Component 無視檢查與回退
        /// </summary>
        /// <param name="go"></param>
        /// <param name="obj"></param>
        public static void AddComponentUncheckedUndoable(GameObject go, UnityEngine.Object obj)
        {
            if (go == null || obj == null)
                return;

            MethodInfo addScriptMethod = typeof(InternalEditorUtility).GetMethod(
               "AddScriptComponentUncheckedUndoable",
               BindingFlags.Static | BindingFlags.NonPublic);
            if (addScriptMethod != null)
                addScriptMethod.Invoke(null, new object[] { go, obj });
        }
    }
}
