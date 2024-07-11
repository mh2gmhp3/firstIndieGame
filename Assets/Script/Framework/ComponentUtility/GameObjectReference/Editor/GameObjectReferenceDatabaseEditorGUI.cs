using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ComponentUtility.GameObjectReferenceDatabase;
using Object = UnityEngine.Object;

namespace ComponentUtility
{
    public class GameObjectReferenceDatabaseEditorGUI
    {
        public struct DragAndDropArea<T>
        {
            public Rect Area;
            public Action<T> Action;

            public DragAndDropArea(Rect area, Action<T> action)
            {
                Area = area;
                Action = action;
            }
        }

        private GameObjectReferenceDatabase _target = null;

        private List<DragAndDropArea<GameObject>> _dragAndDropAreaList = new List<DragAndDropArea<GameObject>>();

        private List<GameObjectReference> _deleteGameObjectRefrence = new List<GameObjectReference>();
        private List<GameObject> _deleteGameObject = new List<GameObject>();

        public GameObjectReferenceDatabaseEditorGUI(GameObjectReferenceDatabase target)
        {
            _target = target;
        }

        public void OnGUI()
        {
            _dragAndDropAreaList.Clear();
            _deleteGameObjectRefrence.Clear();

            EditorGUILayout.BeginVertical();
            {
                var boxArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true), GUILayout.Height(50));
                GUI.Box(boxArea, "拖曳加入新參考");
                GUILayout.Space(10);
                _dragAndDropAreaList.Add(new DragAndDropArea<GameObject>(boxArea, AddGameObject));

                for (int i = 0; i < _target.GameObjectReferenceList.Count; i++)
                {
                    DrawGUIGameObjectReference(_target.GameObjectReferenceList[i], out bool delete);
                    GUILayout.Space(5);
                    if (delete)
                        _deleteGameObjectRefrence.Add(_target.GameObjectReferenceList[i]);
                }

                //Delete
                for (int i = 0; i < _deleteGameObjectRefrence.Count; i++)
                {
                    _target.GameObjectReferenceList.Remove(_deleteGameObjectRefrence[i]);
                }
            }
            EditorGUILayout.EndVertical();

            ProcessDragAndDrop();
        }

        private void DrawGUIGameObjectReference(GameObjectReference gameObjectReference, out bool delete)
        {
            delete = false;
            if (gameObjectReference == null)
                return;

            EditorGUILayout.BeginHorizontal();
            {
                var popCompTypeName = GenGameObjectComponentTypeNameArray(gameObjectReference, out int nowTypeIndex);
                var newTypeIndex = EditorGUILayout.Popup(nowTypeIndex, popCompTypeName);
                if (newTypeIndex != nowTypeIndex)
                    gameObjectReference.TypeName = popCompTypeName[newTypeIndex];
                GUILayout.TextField(gameObjectReference.Name);
                if (GUILayout.Button("X", GUILayout.Width(100)))
                    delete = true;
            }
            EditorGUILayout.EndHorizontal();

            var boxArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true), GUILayout.Height(40));
            GUI.Box(boxArea, "拖曳加入列表");
            _dragAndDropAreaList.Add(new DragAndDropArea<GameObject>(boxArea,
                (obj) =>
                {
                    gameObjectReference.AddGameObject(obj);
                }));

            _deleteGameObject.Clear();
            for (int i = 0; i < gameObjectReference.GameObjectList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    var gameObject = gameObjectReference.GameObjectList[i];
                    EditorGUILayout.ObjectField(gameObject, typeof(GameObject), true);
                    if (GUILayout.Button("X", GUILayout.Width(100)))
                        _deleteGameObject.Add(gameObject);
                }
                EditorGUILayout.EndHorizontal();
            }

            //Delete
            for (int i = 0; i < _deleteGameObject.Count; i++)
            {
                gameObjectReference.GameObjectList.Remove(_deleteGameObject[i]);
            }

            if (gameObjectReference.GameObjectList.Count == 0)
                delete = true;
        }

        private HashSet<string> _compTypeNameSet = new HashSet<string>();
        private List<Component> _compTypeList = new List<Component>();
        private string[] GenGameObjectComponentTypeNameArray(GameObjectReference gameObjectReference, out int nowTypeIndex)
        {
            _compTypeNameSet.Clear();

            _compTypeNameSet.Add(typeof(GameObject).Name);
            for (int i = 0;i < gameObjectReference.GameObjectList.Count;i++)
            {
                gameObjectReference.GameObjectList[i].GetComponents(_compTypeList);
                for (int j = 0; j < _compTypeList.Count; j++)
                {
                    _compTypeNameSet.Add(_compTypeList[j].GetType().Name);
                }
            }

            nowTypeIndex = 0;
            var resultTypeNameArray = _compTypeNameSet.ToArray();
            for (int i = 0; i < resultTypeNameArray.Length; i++)
            {
                if (resultTypeNameArray[i] == gameObjectReference.TypeName)
                {
                    nowTypeIndex = i;
                    break;
                }
            }

            return resultTypeNameArray;
        }

        private void ProcessDragAndDrop()
        {
            Event ev = Event.current;
            switch (ev.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    for (int i = 0; i < _dragAndDropAreaList.Count; i++)
                    {
                        if (!_dragAndDropAreaList[i].Area.Contains(ev.mousePosition))
                            continue;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (ev.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (Object draggedObject in DragAndDrop.objectReferences)
                            {
                                if (draggedObject is GameObject go)
                                {
                                    _dragAndDropAreaList[i].Action.Invoke(go);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void AddGameObject(GameObject newGo)
        {
            _target.AddObject(newGo);
        }
    }
}
