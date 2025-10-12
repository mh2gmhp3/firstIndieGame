using Extension;
using GameMainModule.Animation;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utility;

namespace GameMainModule.Attack
{
    [Serializable]
    public class CharacterAttackController
    {
        [SerializeField]
        private List<AttackCombination> _combinationList =
            new List<AttackCombination>();

        private AttackCombination _nowCombination;

        private ObserverController<IAttackCombinationObserver> _observerController = new ObserverController<IAttackCombinationObserver>();

        private AnimatorController _animatorController;

        public bool IsComboing => _nowCombination != null && _nowCombination.IsComboing;
        public bool IsProcessCombo => _nowCombination != null && _nowCombination.IsProcessingCombo;
        public bool IsMaxCombo => _nowCombination != null && _nowCombination.IsMaxCombo;

        /// <summary>
        /// TODO 用來測試設定攻擊組合 實際在遊戲內應該會由另外一筆玩家編輯資料設定 而不是在運作的類別
        /// </summary>
        /// <param name="combinationList"></param>
        public bool SetCombinationList(List<AttackCombination> combinationList)
        {
            if (IsComboing)
                return false;

            if (combinationList == null)
                return false;

            _combinationList.Clear();
            _combinationList.AddRange(combinationList);
            return true;
        }

        public bool SetNowCombination(int index)
        {
            if (IsComboing)
                return false;

            if (!_combinationList.TryGet(index, out var combinariotn))
                return false;

            if (_nowCombination != null)
            {
                _nowCombination.ClearObserverList();
                _nowCombination.Reset();
            }

            _nowCombination = combinariotn;

            if (_nowCombination != null)
            {
                _nowCombination.AddObserverList(_observerController.ObserverList);

                //替換攻擊動畫
                if (GameMainSystem.AttackBehaviorAssetSetting.TryGetAnimationOverrideNameToClipDic(
                    _nowCombination.WeaponGroup, out var overrides))
                {
                    _animatorController.SetOverride(overrides);
                }
            }

            return true;
        }

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
            if (_nowCombination == null)
                return;

            _nowCombination.DoUpdate(keepComboOnEnd);
        }

        #endregion

        #region Trigger

        public void TriggerMainAttack()
        {
            if (_nowCombination == null)
                return;

            _nowCombination.TriggerMainAttack();
        }

        public void TriggerSubAttack()
        {
            if (_nowCombination == null)
                return;

            _nowCombination.TriggerSubAttack();
        }

        public bool HaveTrigger()
        {
            if (_nowCombination == null)
                return false;

            return _nowCombination.HaveTrigger();
        }

        public void ResetTrigger()
        {
            if (_nowCombination == null)
                return;

            _nowCombination.ResetTrigger();
        }

        #endregion

        #region Reset

        public void Reset()
        {
            if (_nowCombination == null)
                return;

            _nowCombination.Reset();
        }

        public void ResetCombo()
        {
            if (_nowCombination == null)
                return;

            _nowCombination.ResetCombo();
        }

        #endregion

        #region Animation Setting

        public void InitAnimation(AnimatorController animatorController)
        {
            _animatorController = animatorController;
        }

        #endregion
    }
}
