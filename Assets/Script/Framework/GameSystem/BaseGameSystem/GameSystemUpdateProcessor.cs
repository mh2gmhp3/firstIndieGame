using GameSystem.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Framework
{
    public partial class GameSystemManager
    {
        private class GameSystemUpdateProcessor
        {
            /// <summary>
            /// 處理GameSystem Update相關行為
            /// </summary>
            private List<IBaseGameSystem> _updateGameSystemList;
            /// <summary>
            /// 是否啟用
            /// </summary>
            private bool _active = false;

            public GameSystemUpdateProcessor(List<IBaseGameSystem> updateSystemList)
            {
                _updateGameSystemList = new List<IBaseGameSystem>();
                _updateGameSystemList.AddRange(updateSystemList);
                _active = false;
            }

            /// <summary>
            /// 設置啟用
            /// </summary>
            /// <param name="active"></param>
            public void SetActice(bool active)
            {
                _active = active;
            }

            /// <summary>
            /// 處理Update
            /// </summary>
            public void ProcessUpdate()
            {
                if (!_active)
                    return;

                for (int i = 0; i < _updateGameSystemList.Count; i++)
                {
                    IBaseGameSystem baseGameSystem = _updateGameSystemList[i];
                    baseGameSystem.Update();
                }
            }

            /// <summary>
            /// 處理FixedUpdate
            /// </summary>
            public void ProcessFixedUpdate()
            {
                if (!_active)
                    return;

                for (int i = 0; i < _updateGameSystemList.Count; i++)
                {
                    IBaseGameSystem baseGameSystem = _updateGameSystemList[i];
                    baseGameSystem.FixedUpdate();
                }
            }

            /// <summary>
            /// 處理LateUpdate
            /// </summary>
            public void ProcessLateUpdate()
            {
                if (!_active)
                    return;

                for (int i = 0; i < _updateGameSystemList.Count; i++)
                {
                    IBaseGameSystem baseGameSystem = _updateGameSystemList[i];
                    baseGameSystem.LateUpdate();
                }
            }
        }
    }
}
