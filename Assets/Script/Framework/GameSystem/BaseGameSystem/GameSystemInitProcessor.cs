using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Framework
{
    public partial class GameSystemManager
    {
        /// <summary>
        /// 處理GameSystem初始化
        /// </summary>
        private class GameSystemInitProcessor
        {
            /// <summary>
            /// 全部需要處理的GameSystem
            /// </summary>
            private List<IBaseGameSystem> _initGameSystemList;
            /// <summary>
            /// 以處理過的GameSystem
            /// </summary>
            private List<IBaseGameSystem> _cacheInitedGameSystemList;

            public GameSystemInitProcessor(List<IBaseGameSystem> initSystemList)
            {
                _initGameSystemList = new List<IBaseGameSystem>();
                _cacheInitedGameSystemList = new List<IBaseGameSystem>();

                _initGameSystemList.AddRange(initSystemList);
            }

            /// <summary>
            /// 處理初始化
            /// </summary>
            /// <returns></returns>
            public bool ProcessInit()
            {
                //沒有需要初始化的列表當作完成
                if (_initGameSystemList == null)
                    return true;

                for (int i = 0; i < _initGameSystemList.Count; i++)
                {
                    IBaseGameSystem baseGameSystem = _initGameSystemList[i];
                    if (baseGameSystem.Init())
                    {
                        _cacheInitedGameSystemList.Add(baseGameSystem);
                    }
                    else
                    {
                        //有系統無法初始化完成取消此次初始化 等下一次初始化
                        break;
                    }
                }

                //清除掉已完成的
                for (int i = 0; i < _cacheInitedGameSystemList.Count; i++)
                {
                    _initGameSystemList.Remove(_cacheInitedGameSystemList[i]);
                }

                //已完成所有初始化
                if (_initGameSystemList.Count == 0)
                    return true;

                return false;
            }
        }
    }
}
