using GameSystem.Framework.Assets;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Framework
{
    public partial class GameSystemManager
    {
        /// <summary>
        /// 處理GameSystem進入遊戲流程階段
        /// </summary>
        private class GameSystemEnterGameFlowStepProcessor
        {
            /// <summary>
            /// 全部需要處理的GameSystem
            /// </summary>
            private List<IBaseGameSystem> _enterGameFlowStepGameSystemList;
            /// <summary>
            /// 設定檔
            /// </summary>
            private GameSystemEnterGameFlowStepSetting _enterGameFlowStepSetting;

            /// <summary>
            /// 當前階段
            /// </summary>
            private int _nowFlowStep;
            /// <summary>
            /// 當前階段處理中的GameSystem
            /// </summary>
            private List<IBaseGameSystem> _processEnterGameFlowStepGameSystemList;
            /// <summary>
            /// 未處理階段Queue
            /// </summary>
            private Queue<int> _flowStepQueue;

            /// <summary>
            /// 使否啟用
            /// </summary>
            private bool _active = false;

            public GameSystemEnterGameFlowStepProcessor(List<IBaseGameSystem> initSystemList)
            {
                _enterGameFlowStepGameSystemList = new List<IBaseGameSystem>();
                _enterGameFlowStepGameSystemList.AddRange(initSystemList);

                _nowFlowStep = -1;
                _processEnterGameFlowStepGameSystemList = new List<IBaseGameSystem>();
                _flowStepQueue = new Queue<int>();

                _enterGameFlowStepSetting =
                    AssetsSystem.LoadAssets<GameSystemEnterGameFlowStepSetting>(GameSystemEnterGameFlowStepSetting.RESOURCE_FRAMEWORK_PATH);
                if (_enterGameFlowStepSetting == null)
                {
                    Log.LogError("GameSystemEnterGameFlowStepProcessor construct error _enterGameFlowStepSetting is null");
                }
                else
                {
                    for (int i = 0; i < _enterGameFlowStepSetting.EnterGameFlowStep.Count; i++)
                    {
                        _flowStepQueue.Enqueue(_enterGameFlowStepSetting.EnterGameFlowStep[i]);
                    }
                }
            }

            /// <summary>
            /// 設置啟用
            /// </summary>
            /// <param name="active"></param>
            public void SetActive(bool active)
            {
                _active = active;
            }

            /// <summary>
            /// 處理進入遊戲階段
            /// </summary>
            public bool ProcessEnterGameFlowStep()
            {
                if (!_active)
                    return false;

                if (_flowStepQueue.Count == 0 &&
                    _processEnterGameFlowStepGameSystemList.Count == 0)
                {
                    _active = false;
                    return true;
                }

                if (_processEnterGameFlowStepGameSystemList.Count == 0)
                {
                    //處理進入下個階段
                    _processEnterGameFlowStepGameSystemList.AddRange(_enterGameFlowStepGameSystemList);
                    _nowFlowStep = _flowStepQueue.Dequeue();

                    for (int i = 0; i < _processEnterGameFlowStepGameSystemList.Count; i++)
                    {
                        IBaseGameSystem baseGameSystem = _processEnterGameFlowStepGameSystemList[i];
                        baseGameSystem.EnterGameFlowEnterStep(_nowFlowStep);
                    }
                }
                else
                {
                    //等每個System都處裡完當前階段
                    bool allProcessed = true;
                    for (int i = 0; i < _processEnterGameFlowStepGameSystemList.Count; i++)
                    {
                        IBaseGameSystem baseGameSystem = _processEnterGameFlowStepGameSystemList[i];
                        if (!baseGameSystem.EnterGameFlowProcessStep(_nowFlowStep))
                            allProcessed = false;
                    }
                    if (allProcessed)
                        _processEnterGameFlowStepGameSystemList.Clear();
                }

                return false;
            }
        }
    }
}
