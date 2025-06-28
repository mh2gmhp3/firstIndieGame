using Extension;
using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Attack
{
    //TODO 先對應一般連擊用的組合 之後有需要對遠程子彈編輯在處理
    [Serializable]
    public class AttackCombination
    {
        [SerializeField]
        private List<AttackBehavior> _mainAttackBehaviorList = new List<AttackBehavior>();
        [SerializeField]
        private List<AttackBehavior> _subAttackBehaviorList = new List<AttackBehavior>();

        private AttackBehavior _nowAttackBehavior = null;
        private AttackBehavior _nextAttackBehavior = null;

        [SerializeField]
        private bool _isStartNewComboBehavior = false;
        [SerializeField]
        private int _currentComboIndex = -1;

        public Action _onStartComboing;
        public Action _onEndComboing;

        public IAttackAnimationController _aniController;

        public bool IsComboing => _nowAttackBehavior != null;

        public AttackCombination()
        {

        }

        public AttackCombination(List<AttackBehavior> mainAttackBehaviorList, List<AttackBehavior> subAttackBehaviorList)
        {
            _mainAttackBehaviorList = mainAttackBehaviorList;
            _subAttackBehaviorList = subAttackBehaviorList;
        }

        #region Notify Combo

        public void RegisterComboingAction(Action onStart, Action onEnd)
        {
            _onStartComboing = onStart;
            _onEndComboing = onEnd;
        }

        public void ClearComboingAction()
        {
            _onStartComboing = null;
            _onEndComboing = null;
        }

        private void InvokeStartComboing()
        {
            if (_onStartComboing != null)
            {
                _onStartComboing.Invoke();
            }
        }

        private void InvokeEndComboing()
        {
            if (_onEndComboing != null)
            {
                _onEndComboing.Invoke();
            }
        }

        #endregion

        #region Notify Animation

        public void SetAnimationController(IAttackAnimationController controller)
        {
            _aniController = controller;
        }

        public void ClearAnimationController()
        {
            _aniController = null;
        }

        #endregion

        public void TriggerMainAttack()
        {
            TriggerAttack(_mainAttackBehaviorList);
        }

        public void TriggerSubAttack()
        {
            TriggerAttack(_subAttackBehaviorList);
        }

        public void Reset()
        {
            _isStartNewComboBehavior = false;
            _currentComboIndex = -1;

            if (_nowAttackBehavior != null)
                _nowAttackBehavior.Reset();
            _nowAttackBehavior = null;
            _nextAttackBehavior = null;
        }

        public void DoUpdate()
        {
            if (_nowAttackBehavior == null)
                return;

            if (_isStartNewComboBehavior)
            {
                if (_currentComboIndex == 0)
                {
                    InvokeStartComboing();
                }
                _nowAttackBehavior.OnStart();
                if (_aniController != null)
                {
                    _aniController.OnAttack(_nowAttackBehavior.Name);
                }
                Log.LogInfo("Start Attack Behavior : " + _nowAttackBehavior.Name);
                _isStartNewComboBehavior = false;
            }

            _nowAttackBehavior.OnUpdate();

            if (_nowAttackBehavior.IsEnd)
            {
                _nowAttackBehavior.OnEnd();
                Log.LogInfo("End Attack Behavior : " + _nowAttackBehavior.Name);
                //沒有輸入下一個 停止
                if (_nextAttackBehavior == null)
                {
                    _nowAttackBehavior = null;
                    InvokeEndComboing();
                }
                else
                {
                    _nowAttackBehavior = _nextAttackBehavior;
                    _nextAttackBehavior = null;
                    _isStartNewComboBehavior = true;
                }
            }
        }

        private void TriggerAttack(List<AttackBehavior> attackBehaviorList)
        {
            if (attackBehaviorList == null)
                return;

            if (NeedStartNewCombo())
            {
                StartNewCombo(attackBehaviorList);
            }
            else
            {
                if (!_nowAttackBehavior.CanNextBehavior)
                {
                    Log.LogInfo("Attack Behavior Lock Next : " + _nowAttackBehavior.Name);
                    return;
                }

                if (_nextAttackBehavior != null)
                    return;

                _currentComboIndex++;
                if (!attackBehaviorList.TryGet(_currentComboIndex, out var result))
                    return;

                _nextAttackBehavior = result;
            }
        }

        private bool NeedStartNewCombo()
        {
            return _nowAttackBehavior == null;
        }

        private void StartNewCombo(List<AttackBehavior> attackBehaviorList)
        {
            if (!attackBehaviorList.TryGetFirst(out var firstBehavior))
                return;

            _nowAttackBehavior = firstBehavior;
            _isStartNewComboBehavior = true;
            _currentComboIndex = 0;
        }
    }
}
