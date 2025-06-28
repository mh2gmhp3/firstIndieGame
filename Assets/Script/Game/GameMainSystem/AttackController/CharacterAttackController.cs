using Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace GameMainModule.Attack
{
    [Serializable]
    public class CharacterAttackController
    {
        private const int NO_ATTACK_COMBO_COUNT = 0;

        [SerializeField]
        private List<AttackCombination> _combinationList =
            new List<AttackCombination>();

        private AttackCombination _nowCombination;

        [SerializeField]
        private int _attackComboCount = NO_ATTACK_COMBO_COUNT;

        private Action _onStartComboing;
        private Action _onEndComboing;

        public bool IsComboing => _nowCombination != null && _nowCombination.IsComboing;

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
                _nowCombination.ClearComboingAction();
            }

            _nowCombination = combinariotn;

            if (_nowCombination != null)
            {
                _nowCombination.RegisterComboingAction(_onStartComboing, _onEndComboing);
            }

            return true;
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

        #endregion

        public void DoUpdate()
        {
            if (_nowCombination == null)
                return;

            _nowCombination.DoUpdate();
        }

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
    }
}
