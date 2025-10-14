using Extension;
using GameMainModule.Animation;
using Logging;
using System;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;
using Utility;

namespace GameMainModule.Attack
{
    public interface IAttackCombinationSetupData
    {
        public int GroupId { get; }
        public List<int> AttackBehaviorList { get; }
    }

    [Serializable]
    public class CharacterAttackController
    {
        [SerializeField]
        private List<AttackCombination> _combinationList =
            new List<AttackCombination>();

        private AttackCombination _curCombination;

        private ObserverController<IAttackCombinationObserver> _observerController = new ObserverController<IAttackCombinationObserver>();

        private UnitMovementSetting _unitMovementSetting;
        private AnimatorController _animatorController;

        private int _curCombinationIndex = -1;
        private int _combinationMaxCount = 0;

        public bool IsComboing => _curCombination != null && _curCombination.IsComboing;
        public bool IsProcessCombo => _curCombination != null && _curCombination.IsProcessingCombo;
        public bool IsMaxCombo => _curCombination != null && _curCombination.IsMaxCombo;

        public void Init(UnitMovementSetting unitMovementSetting, AnimatorController animatorController)
        {
            _unitMovementSetting = unitMovementSetting;
            _animatorController = animatorController;
        }

        #region Comnination

        public void SetCombinationMaxCount(int count)
        {
            _combinationMaxCount = count;
            _combinationList.EnsureCount(_combinationMaxCount, () => { return new AttackCombination(_unitMovementSetting); }, true);
        }

        public bool SetCombination(int index, AttackCombinationRuntimeSetupData setupData)
        {
            if (IsComboing)
                return false;

            if (index < 0 || index >= _combinationList.Count)
                return false;

            Log.LogInfo($"CharacterAttackController SetCombination Index:{index}");
            _combinationList[index].Update(setupData);

            // 相同 直接重設刷新
            if (index == _curCombinationIndex)
                SetCurCombination(index);

            return true;
        }

        public bool SetCurCombination(int index)
        {
            if (IsComboing)
                return false;

            if (!_combinationList.TryGet(index, out var combination))
                return false;

            _curCombinationIndex = index;

            if (_curCombination != null)
            {
                _curCombination.ClearObserverList();
                _curCombination.Reset();
            }

            _curCombination = combination;

            if (_curCombination != null)
            {
                _curCombination.AddObserverList(_observerController.ObserverList);

                //替換攻擊動畫
                if (GameMainSystem.AttackBehaviorAssetSetting.TryGetAnimationOverrideNameToClipDic(
                    _curCombination.WeaponGroup, out var overrides))
                {
                    _animatorController.SetOverride(overrides);
                }
            }

            return true;
        }

        #endregion

        #region IAttackCombinationObserver

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
            _observerController.ClearObservers();
        }

        #endregion

        #region ProcessCombo

        public void DoUpdate(bool keepComboOnEnd)
        {
            if (_curCombination == null)
                return;

            _curCombination.DoUpdate(keepComboOnEnd);
        }

        #endregion

        #region Trigger

        public void TriggerMainAttack()
        {
            if (_curCombination == null)
                return;

            _curCombination.TriggerMainAttack();
        }

        public void TriggerSubAttack()
        {
            if (_curCombination == null)
                return;

            _curCombination.TriggerSubAttack();
        }

        public bool HaveTrigger()
        {
            if (_curCombination == null)
                return false;

            return _curCombination.HaveTrigger();
        }

        public void ResetTrigger()
        {
            if (_curCombination == null)
                return;

            _curCombination.ResetTrigger();
        }

        #endregion

        #region Reset

        public void Reset()
        {
            if (_curCombination == null)
                return;

            _curCombination.Reset();
        }

        public void ResetCombo()
        {
            if (_curCombination == null)
                return;

            _curCombination.ResetCombo();
        }

        #endregion
    }
}
