using Framework.Editor.Utility;
using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static Framework.ComponentUtility.ObjectReferenceDatabase;
using Object = UnityEngine.Object;

namespace Framework.ComponentUtility.Editor
{
    public class ObjectReferenceDatabaseEditorGUI
    {
        private ObjectReferenceDatabase _target = null;

        private DragAndDropHandler<Object> _dragAndDropHandler = new DragAndDropHandler<Object>();

        private List<ObjectReference> _deleteObjectRefrence = new List<ObjectReference>();
        private List<Object> _deleteObj = new List<Object>();

        public ObjectReferenceDatabaseEditorGUI(ObjectReferenceDatabase target)
        {
            _target = target;
        }

        public void OnGUI()
        {
            _dragAndDropHandler.ClearArea();
            _deleteObjectRefrence.Clear();

            EditorGUILayout.BeginVertical();
            {
                var boxArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true), GUILayout.Height(50));
                GUI.Box(boxArea, "拖曳加入新參考");
                GUILayout.Space(10);
                _dragAndDropHandler.AddArea(boxArea, AddObjectReference);

                for (int i = 0; i < _target.ObjectReferenceList.Count; i++)
                {
                    DrawGUIObjectReference(_target.ObjectReferenceList[i], out bool delete);
                    GUILayout.Space(5);
                    if (delete)
                        _deleteObjectRefrence.Add(_target.ObjectReferenceList[i]);
                }

                //Delete
                for (int i = 0; i < _deleteObjectRefrence.Count; i++)
                {
                    _target.ObjectReferenceList.Remove(_deleteObjectRefrence[i]);
                }
            }
            EditorGUILayout.EndVertical();

            _dragAndDropHandler.ProcessDragAndDrop();
        }

        private void DrawGUIObjectReference(ObjectReference objectReference, out bool delete)
        {
            delete = false;
            if (objectReference == null)
                return;

            EditorGUILayout.BeginHorizontal();
            {
                var typeList = GetObjectComponentTypeList(objectReference);
                var popCompTypeNameList = typeList.Select((t) => t.Name).ToList();
                var nowTypeIndex = popCompTypeNameList.FindIndex((t) => t == objectReference.TypeName);
                var newTypeIndex = EditorGUILayout.Popup(nowTypeIndex, popCompTypeNameList.ToArray());
                if (newTypeIndex != nowTypeIndex)
                {
                    var newType = typeList[newTypeIndex];
                    if (!_target.DuplicateTypeAndName(newType, objectReference.Name))
                        objectReference.ChangeType(typeList[newTypeIndex]);
                    else
                        Log.LogError("無法使用此類型，已有重複類型與名稱");
                }
                var newName = GUILayout.TextField(objectReference.Name);
                if (objectReference.Name != newName)
                {
                    var nowType = typeList[newTypeIndex];
                    if (!_target.DuplicateTypeAndName(nowType, newName))
                        objectReference.Name = newName;
                    else
                        Log.LogError("無法命名，已有重複類型與名稱");
                }
                if (GUILayout.Button("X", GUILayout.Width(100)))
                    delete = true;
            }
            EditorGUILayout.EndHorizontal();

            var boxArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true), GUILayout.Height(40));
            GUI.Box(boxArea, "拖曳加入列表");
            _dragAndDropHandler.AddArea(
                boxArea,
                (obj) =>
                {
                    objectReference.AddObject(obj);
                });

            _deleteObj.Clear();
            for (int i = 0; i < objectReference.ObjectList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var obj = objectReference.ObjectList[i];
                    var objType = obj.GetType();
                    EditorGUILayout.ObjectField(obj, objType, true);
                    if (GUILayout.Button("X", GUILayout.Width(100)))
                        _deleteObj.Add(obj);
                }
                EditorGUILayout.EndHorizontal();
            }

            //Delete
            for (int i = 0; i < _deleteObj.Count; i++)
            {
                objectReference.ObjectList.Remove(_deleteObj[i]);
            }

            if (objectReference.ObjectList.Count == 0)
                delete = true;
        }

        private List<Type> _compTypeList = new List<Type>();
        private List<Type> GetObjectComponentTypeList(ObjectReference objectReference)
        {
            _compTypeList.Clear();

            var goType = typeof(GameObject);
            _compTypeList.Add(goType);
            for (int i = 0; i < objectReference.ObjectList.Count; i++)
            {
                var compTypes = objectReference.ObjectList[i].GetComponents<Component>();
                for (int j = 0; j < compTypes.Length; j++)
                {
                    var type = compTypes[j].GetType();
                    _compTypeList.Add(type);
                }
            }

            return _compTypeList;
        }

        private void AddObjectReference(Object newObjRef)
        {
            _target.AddObjectReference(newObjRef);
        }
    }
}
