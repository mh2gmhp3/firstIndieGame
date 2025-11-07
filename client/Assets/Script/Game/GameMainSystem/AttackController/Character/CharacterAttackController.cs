using AssetModule;
using Extension;
using FormModule;
using GameMainModule.Animation;
using Logging;
using System;
using System.Collections.Generic;
using UnitModule;
using UnitModule.Movement;
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

        private AttackCombination _curCombination;

        private ObserverController<IAttackCombinationObserver> _observerController = new ObserverController<IAttackCombinationObserver>();

        private CharacterAttackRefSetting _attackRefSetting;
        private CharacterPlayableClipController _playableClipController;
        private WeaponTransformSetting _weaponTransformSetting;

        private GameObject _curWeaponObj;
        private bool _isWeaponActive;

        private int _curCombinationIndex = -1;
        private int _combinationMaxCount = 0;

        public bool IsComboing => _curCombination != null && _curCombination.IsComboing;
        public bool IsProcessCombo => _curCombination != null && _curCombination.IsProcessingCombo;
        public bool IsMaxCombo => _curCombination != null && _curCombination.IsMaxCombo;

        public void Init(
            CharacterAttackRefSetting attackRefSetting,
            CharacterPlayableClipController playableClipController,
            WeaponTransformSetting weaponTransformSetting)
        {
            _attackRefSetting = attackRefSetting;
            _playableClipController = playableClipController;
            _weaponTransformSetting = weaponTransformSetting;
        }

        #region Comnination

        public void SetCombinationMaxCount(int count)
        {
            _combinationMaxCount = count;
            _combinationList.EnsureCount(_combinationMaxCount, () => { return new AttackCombination(_attackRefSetting); }, true);
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
                SetWeaponAnimation(_curCombination.WeaponGroup);
                //替換武器模型
                SetWeaponModel(_curCombination.WeaponSettingId);
            }

            return true;
        }

        private void SetWeaponAnimation(int weaponGroup)
        {
            if (!GameMainSystem.AttackBehaviorAssetSetting.TryGetAnimationNameToClipDic(
                weaponGroup, out var attackNameToClip))
            {
                return;
            }

            _playableClipController.SetAttackClip(attackNameToClip);
        }

        private void SetWeaponModel(int weaponSettingId)
        {
            if (_curWeaponObj != null)
            {
                //先直接移除 應該可以暫存起來
                UnityEngine.Object.Destroy(_curWeaponObj);
            }

            if (!FormSystem.Table.WeaponTable.TryGetData(weaponSettingId, out var weaponRow))
            {
                return;
            }

            var weaponAssets = AssetSystem.LoadAsset<GameObject>(AssetPathUtility.GetWeaponModelPath(weaponRow.ModelName));
            if (weaponAssets == null)
                return;

            var weaponGo = UnityEngine.Object.Instantiate(weaponAssets);
            _curWeaponObj = weaponGo;
            SetWeaponActive(_isWeaponActive);
        }

        public void SetWeaponActive(bool active)
        {
            _isWeaponActive = active;
            if (_curWeaponObj == null)
                return;

            if (!FormSystem.Table.WeaponTable.TryGetData(_curCombination.WeaponSettingId, out var weaponRow))
            {
                return;
            }

            Transform weaponParent = null;
            if (_isWeaponActive)
            {
                if (weaponRow.UseHand == 0)
                    weaponParent = _weaponTransformSetting.Left_HandTransform;
                else if (weaponRow.UseHand == 1)
                    weaponParent = _weaponTransformSetting.Right_HandTransform;
            }
            else
            {
                if (!_weaponTransformSetting.StorageTransformList.TryGet(weaponRow.StorageIndex, out var storageTrans))
                    return;

                weaponParent = storageTrans;
            }

            if (weaponParent == null)
                return;

            _curWeaponObj.transform.SetParent(weaponParent, false);
            _curWeaponObj.transform.Reset();
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
