using Extension;
using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace GameMainModule.Attack
{
    public interface IAttackCombinationObserver
    {
        void OnStartAttackBehavior(string behaviorName);

        void OnStartComboing();
        void OnEndComboing();
    }

    //TODO 先對應一般連擊用的組合 之後有需要對遠程子彈編輯在處理
    [Serializable]
    public class AttackCombination
    {
        private List<AttackBehavior> _mainAttackBehaviorList = new List<AttackBehavior>();
        private List<AttackBehavior> _subAttackBehaviorList = new List<AttackBehavior>();

        private AttackBehavior _nowAttackBehavior = null;
        private AttackBehavior _nextAttackBehavior = null;

        /// <summary>
        /// 可開關保留Combo狀態<see cref="KeepComboing(bool)"/>
        /// <para>開啟時: 不會因為不輸入斷掉Combo，下次輸入時繼續Combo，最終Combo後繼續輸入不會重新Combo，必須關閉。Combo開始與繼續都會通知開始 沒輸入導致Combo中斷通知結束</para>
        /// <para>關閉時: 不輸入斷掉Combo。 Combo開始通知開始 Combo結束通知結束</para>
        /// </summary>
        private bool _keepComboing = false;
        private bool _isKeeping = false;

        /// <summary>
        /// 控制Combo開始與結束通知
        /// </summary>
        private bool _isProcessingCombo = false;
        private int _currentComboIndex = -1;

        private bool _mainTrigger = false;
        private bool _subTriggger = false;

        private ObserverController<IAttackCombinationObserver> _observerController = new ObserverController<IAttackCombinationObserver>();

        public bool IsComboing => _nowAttackBehavior != null || _nextAttackBehavior != null;
        public bool IsProcessingCombo => _isProcessingCombo;

        public AttackCombination()
        {

        }

        public AttackCombination(List<AttackBehavior> mainAttackBehaviorList, List<AttackBehavior> subAttackBehaviorList)
        {
            _mainAttackBehaviorList = mainAttackBehaviorList;
            _subAttackBehaviorList = subAttackBehaviorList;
        }

        #region IAttackCombinationObserver

        public void AddObserverList(List<IAttackCombinationObserver> observerList)
        {
            _observerController.AddObservers(observerList);
        }

        public void AddObserver(IAttackCombinationObserver observer)
        {
            _observerController.AddObserver(observer);
        }

        public void RemoveObserver(IAttackCombinationObserver observer)
        {
            _observerController.RemoveObserver(observer);
        }

        public void ClearObserverList()
        {
            _observerController.ClearObservers();;
        }

        private void NotifyStartAttackBehavior(string behaviorName)
        {
            for (int i = 0; i < _observerController.ObserverList.Count; i++)
            {
                var observer = _observerController.ObserverList[i];
                if (observer != null)
                {
                    observer.OnStartAttackBehavior(behaviorName);
                }
            }
        }

        private void NotifyStartComboing()
        {
            for (int i = 0; i < _observerController.ObserverList.Count; i++)
            {
                var observer = _observerController.ObserverList[i];
                if (observer != null)
                {
                    observer.OnStartComboing();
                }
            }
        }

        private void NotifyEndComboing()
        {
            for (int i = 0; i < _observerController.ObserverList.Count; i++)
            {
                var observer = _observerController.ObserverList[i];
                if (observer != null)
                {
                    observer.OnEndComboing();
                }
            }
        }

        #endregion

        #region Trigger

        public void TriggerMainAttack()
        {
            _mainTrigger = true;
        }

        public void TriggerSubAttack()
        {
            _subTriggger = true;
        }

        public void ProcessTrigger()
        {
            if (_mainTrigger)
            {
                TriggerAttack(_mainAttackBehaviorList);
                ResetTrigger();
            }
            else if (_subTriggger)
            {
                TriggerAttack(_subAttackBehaviorList);
                ResetTrigger();
            }
        }

        public bool HaveTrigger()
        {
            return _mainTrigger || _subTriggger;
        }

        public void ResetTrigger()
        {
            _mainTrigger = false;
            _subTriggger = false;
        }

        #endregion

        public void Reset()
        {
            _keepComboing = false;
            _isKeeping = false;
            _isProcessingCombo = false;
            _currentComboIndex = -1;
            ResetTrigger();

            if (_nowAttackBehavior != null)
                _nowAttackBehavior.Reset();
            _nowAttackBehavior = null;
            _nextAttackBehavior = null;
        }

        public void DoUpdate()
        {
            ProcessTrigger();

            if (_nowAttackBehavior == null)
            {
                if (_nextAttackBehavior == null)
                    return;

                _nowAttackBehavior = _nextAttackBehavior;
                _nextAttackBehavior = null;

                //每次有新的
                _isKeeping = false;
                _nowAttackBehavior.OnStart();
                if (!_isProcessingCombo)
                {
                    NotifyStartComboing();
                    Log.LogInfo("NotifyStartComboing");
                    _isProcessingCombo = true;
                }
                NotifyStartAttackBehavior(_nowAttackBehavior.Name);
            }

            if (_isKeeping)
            {
                if (_nextAttackBehavior == null)
                {
                    if (_keepComboing)
                        return;

                    // 不保留後 沒有下一個行為就清空
                    _nowAttackBehavior = null;
                    _currentComboIndex = -1;
                }
                else
                {
                    // 有下一個行為就繼續下一個行為
                    _nowAttackBehavior = null;
                }
            }
            else if (_nowAttackBehavior.IsEnd)
            {
                _nowAttackBehavior.OnEnd();
                if (_nextAttackBehavior == null)
                {
                    NotifyEndComboing();
                    _isProcessingCombo = false;
                    Log.LogInfo("NotifyEndComboing");
                    if (_keepComboing)
                    {
                        _isKeeping = true;
                    }
                    else
                    {
                        // 不保留連段 清空
                        _nowAttackBehavior = null;
                        _currentComboIndex = -1;
                    }
                }
                else
                {
                    // 清掉當前行為在下一偵開始下個行為
                    _nowAttackBehavior = null;
                }
            }
            else
            {
                _nowAttackBehavior.OnUpdate();
            }
        }

        private void TriggerAttack(List<AttackBehavior> attackBehaviorList)
        {
            if (attackBehaviorList == null)
                return;

            if (_nowAttackBehavior != null && !_nowAttackBehavior.CanNextBehavior)
                return;

            _currentComboIndex++;
            //allready last Comboing
            if (!attackBehaviorList.TryGet(_currentComboIndex, out var nextBehavior))
                return;

            _nextAttackBehavior = nextBehavior;
        }

        public void KeepComboing(bool keep)
        {
            _keepComboing = keep;
        }
    }
}
