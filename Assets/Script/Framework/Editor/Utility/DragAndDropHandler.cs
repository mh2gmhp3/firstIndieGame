using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Editor.Utility
{
    /// <summary>
    /// 拖曳行為管理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DragAndDropHandler<T>
    {
        /// <summary>
        /// 拖曳區域結構
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public struct DragAndDropArea
        {
            public Rect Area;
            public Action<T> Action;

            public DragAndDropArea(Rect area, Action<T> action)
            {
                Area = area;
                Action = action;
            }
        }

        /// <summary>
        /// 拖曳區域
        /// </summary>
        private List<DragAndDropArea> _areaList = new List<DragAndDropArea>();

        /// <summary>
        /// 清空拖曳區域
        /// </summary>
        public void ClearArea()
        {
            _areaList.Clear();
        }

        /// <summary>
        /// 新增拖曳區域
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="action"></param>
        public void AddArea(Rect rect, Action<T> action)
        {
            _areaList.Add(new DragAndDropArea(rect, action));
        }

        /// <summary>
        /// 處裡拖曳行為
        /// </summary>
        public void ProcessDragAndDrop()
        {
            Event ev = Event.current;
            switch (ev.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    for (int i = 0; i < _areaList.Count; i++)
                    {
                        if (!_areaList[i].Area.Contains(ev.mousePosition))
                            continue;

                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (ev.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();

                            foreach (Object draggedObject in DragAndDrop.objectReferences)
                            {
                                if (draggedObject is T obj)
                                {
                                    _areaList[i].Action.Invoke(obj);
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}
