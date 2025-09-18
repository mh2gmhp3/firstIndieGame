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
        private int _weaponGroup = 0;

        private List<AttackBehavior> _mainAttackBehaviorList = new List<AttackBehavior>();
        private List<AttackBehavior> _subAttackBehaviorList = new List<AttackBehavior>();

        private AttackBehavior _nowAttackBehavior = null;
        private AttackBehavior _nextAttackBehavior = null;

        /// <summary>
        /// 控制Combo開始與結束通知
        /// </summary>
        private bool _isProcessingCombo = false;
        private int _currentComboIndex = -1;

        private bool _mainTrigger = false;
        private bool _subTriggger = false;

        private ObserverController<IAttackCombinationObserver> _observerController = new ObserverController<IAttackCombinationObserver>();

        public int WeaponGroup => _weaponGroup;

        public bool IsComboing => _nowAttackBehavior != null || _nextAttackBehavior != null;
        public bool IsProcessingCombo => _isProcessingCombo;

        public bool IsMaxCombo
        {
            get
            {
                if (_mainTrigger)
                {
                    return _currentComboIndex >= _mainAttackBehaviorList.Count;
                }
                else if (_subTriggger)
                {
                    return _currentComboIndex >= _subAttackBehaviorList.Count;
                }
                return true;
            }
        }

        public AttackCombination()
        {

        }

        public AttackCombination(int weaponGroup, List<AttackBehavior> mainAttackBehaviorList, List<AttackBehavior> subAttackBehaviorList)
        {
            _weaponGroup = weaponGroup;
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

        #region ProcessCombo

        public void DoUpdate(bool keepComboOnEnd)
        {
            ProcessTrigger();

            if (_nowAttackBehavior != null)
            {
                //TODO考慮手感的問題 是不是在可以輸入時就直接End並跳下一個還是真的要等到結束
                //現在在切換到其他的State時都是必須要等到ProcessingCombo結束
                //之後有可能會有突然中斷的時候 自我行為:衝刺或跳用等迴避類行為，被動行為:擊飛或訂身等控場行為
                if (_nowAttackBehavior.IsEnd)
                {
                    _nowAttackBehavior.OnEnd();
                    if (_nextAttackBehavior == null)
                    {
                        EndProcessCombo(keepComboOnEnd);
                    }
                    else
                    {
                        StartProcessComboAndNextBehavior();
                    }
                }
                else
                {
                    _nowAttackBehavior.OnUpdate();
                }
            }
            else if (_nextAttackBehavior != null)
            {
                StartProcessComboAndNextBehavior();
            }
        }

        private void StartProcessComboAndNextBehavior()
        {
            if (_nextAttackBehavior == null)
                return;

            if (!_isProcessingCombo)
            {
                _isProcessingCombo = true;
                NotifyStartComboing();
                Log.LogInfo("NotifyStartComboing");
            }

            _nowAttackBehavior = _nextAttackBehavior;
            _nowAttackBehavior.OnStart();
            NotifyStartAttackBehavior(_nowAttackBehavior.Name);
            Log.LogInfo($"NotifyStartBehaviorName:{_nowAttackBehavior.Name}");
            _nextAttackBehavior = null;
        }

        private void EndProcessCombo(bool keepComboOnEnd)
        {
            if (!_isProcessingCombo)
                return;

            _isProcessingCombo = false;
            NotifyEndComboing();
            Log.LogInfo("NotifyEndComboing");
            _nowAttackBehavior = null;
            if (!keepComboOnEnd)
                _currentComboIndex = -1;
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

        public bool HaveTrigger()
        {
            return _mainTrigger || _subTriggger;
        }

        public void ResetTrigger()
        {
            _mainTrigger = false;
            _subTriggger = false;
        }

        private void ProcessTrigger()
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

        private void TriggerAttack(List<AttackBehavior> attackBehaviorList)
        {
            if (attackBehaviorList == null)
                return;

            if (_nextAttackBehavior != null)
                return;

            if (_nowAttackBehavior != null && !_nowAttackBehavior.CanNextBehavior)
                return;

            _currentComboIndex++;
            //allready last Comboing
            if (!attackBehaviorList.TryGet(_currentComboIndex, out var nextBehavior))
                return;

            _nextAttackBehavior = nextBehavior;
        }

        #endregion

        #region Reset

        public void Reset()
        {
            ResetCombo();
            ResetTrigger();
        }

        public void ResetCombo()
        {
            _isProcessingCombo = false;
            _currentComboIndex = -1;
            if (_nowAttackBehavior != null)
                _nowAttackBehavior.Reset();
            _nowAttackBehavior = null;
            _nextAttackBehavior = null;
        }

        #endregion
    }
}
