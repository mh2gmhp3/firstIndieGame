using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Utility;

namespace Framework.ComponentUtility.Editor
{
    [CreateAssetMenu(
        fileName = "ObjectReferenceDatabaseBaseTypeEventSetting",
        menuName = "ComponentUtility/ObjectReference/ObjectReferenceDatabaseBaseTypeEventSetting")]
    public class ObjectReferenceDatabaseBaseTypeEventSetting : ScriptableObject
    {
        private static ObjectReferenceDatabaseBaseTypeEventSetting Instance
        {
            get
            {
                if (_instance == null)
                {
                    var guids = AssetDatabase.FindAssets($"t:Script {nameof(ObjectReferenceDatabaseBaseTypeEventSetting)}");
                    var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    var dirPath = Path.GetDirectoryName(path);
                    var assetPath = Path.Combine(dirPath, fileName) + ".asset";
                    _instance = AssetDatabase.LoadAssetAtPath<ObjectReferenceDatabaseBaseTypeEventSetting>(assetPath);
                }

                return _instance;
            }
        }

        private static ObjectReferenceDatabaseBaseTypeEventSetting _instance;

        [SerializeField]
        private List<string> _baseTypeFullNameList = new List<string>();

        /// <summary>
        /// 是否使用基類搜尋事件註冊
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool TryGetBaseTypeToSearchEvent(Type type, out List<Type> eventBaseTypeList)
        {
            eventBaseTypeList = null;

            if (type == null)
                return false;

            eventBaseTypeList = new List<Type>();
            var baseTypeList = TypeUtility.GetBaseTypeList(type);
            foreach (var baseType in baseTypeList)
            {
                var typeFullName = baseType.FullName;
                if (Instance._baseTypeFullNameList.Contains(typeFullName))
                    eventBaseTypeList.Add(baseType);
            }

            return eventBaseTypeList.Count > 0;
        }
    }
}
