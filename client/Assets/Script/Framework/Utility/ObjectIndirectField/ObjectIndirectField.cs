using AssetModule;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Utility;

namespace Utility
{
    [Serializable]
    public class ObjectIndirectField<T> where T : UnityEngine.Object
    {
        public string Path;
        public string ResourcePath;

#if UNITY_EDITOR
        [SerializeField]
        private string _guid;
#endif

#if UNITY_EDITOR
        private T _editorInstance;
        public T EditorInstance
        {
            get
            {
                if (_editorInstance == null || AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_editorInstance)) != _guid)
                {
                    var path = AssetDatabase.GUIDToAssetPath(_guid);
                    _editorInstance = AssetDatabase.LoadAssetAtPath<T>(path);
                }
                return _editorInstance;
            }
            set
            {
                var path = AssetDatabase.GetAssetPath(value);
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(guid))
                    return;
                Path = path;
                ResourcePath = AssetPathUtility.AssetPathToResourcesPath(path);
                _guid = guid;
                _editorInstance = value;
            }
        }

        public void CopyTo(ObjectIndirectField<T> target)
        {
            target.Path = Path;
            target.ResourcePath = ResourcePath;
            target._guid = _guid;
        }
#endif
    }
}
