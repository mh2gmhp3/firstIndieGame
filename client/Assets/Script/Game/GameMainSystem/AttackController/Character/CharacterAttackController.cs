using AssetModule;
using CollisionModule;
using Extension;
using FormModule;
using GameMainModule.Animation;
using Logging;
using System;
using System.Collections.Generic;
using UnitModule;
using UnityEngine;

namespace GameMainModule.Attack
{
    public struct AttackBehaviorInfo
    {
        public int RefItemId;
        public AttackBehaviorTimelineRuntimeAsset TimelineAsset;

        public AttackBehaviorInfo(int refItemId, AttackBehaviorTimelineRuntimeAsset timelineAsset)
        {
            RefItemId = refItemId;
            TimelineAsset = timelineAsset;
        }
    }

    [Serializable]
    public class CharacterAttackController : ITimelineListener
    {
        [SerializeField]
        private List<AttackCombination> _combinationList =
            new List<AttackCombination>();

        private CharacterAttackRefSetting _attackRefSetting;
        private CharacterPlayableClipController _playableClipController;
        private AttackBehaviorTimelinePlayController _attackBehaviorTimelinePlayController = new AttackBehaviorTimelinePlayController();
        private FakeCharacterTriggerInfo _fakeCharacterTriggerInfo = new FakeCharacterTriggerInfo();

        private WeaponTransformSetting _weaponTransformSetting;

        private GameObject _curWeaponObj;
        private bool _isWeaponActive;

        private int _curCombinationIndex = -1;
        private int _combinationMaxCount = 0;

        private int _currentComboIndex = -1;

        private bool _mainTrigger = false;
        private bool _subTrigger = false;

        private bool _isProcessingCombo = false;

        public bool IsProcessCombo => _isProcessingCombo;
        public bool IsMaxCombo
        {
            get
            {
                if (!TryCurCombination(out var combination))
                    return true;

                if (_mainTrigger)
                {
                    return _currentComboIndex >= combination.MainAttackBehaviorList.Count;
                }
                else if (_subTrigger)
                {
                    return _currentComboIndex >= combination.SubAttackBehaviorList.Count;
                }
                return true;
            }
        }

        public void Init(
            CharacterAttackRefSetting attackRefSetting,
            CharacterPlayableClipController playableClipController,
            WeaponTransformSetting weaponTransformSetting)
        {
            _attackRefSetting = attackRefSetting;
            _playableClipController = playableClipController;
            _weaponTransformSetting = weaponTransformSetting;
            _attackBehaviorTimelinePlayController.AddListener(this);
        }

        #region Comnination

        public void SetCombinationMaxCount(int count)
        {
            _combinationMaxCount = count;
            _combinationList.EnsureCount(_combinationMaxCount, () => { return new AttackCombination(); }, true);
        }

        public bool SetCombination(int index, AttackCombinationRuntimeSetupData setupData)
        {
            if (IsProcessCombo)
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
            if (IsProcessCombo)
                return false;

            if (!_combinationList.TryGet(index, out var combination))
                return false;

            _curCombinationIndex = index;

            //替換攻擊動畫
            SetWeaponAnimation();
            //替換武器模型
            SetWeaponModel(combination.WeaponSettingId);

            return true;
        }

        private void SetWeaponAnimation()
        {
            if (!TryCurCombination(out var combination))
                return;

            _playableClipController.SetAttackClip(combination.AllAttackClipCache);
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

            if (!TryCurCombination(out var combination))
                return;

            if (!FormSystem.Table.WeaponTable.TryGetData(combination.WeaponSettingId, out var weaponRow))
                return;

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

        private bool TryCurCombination(out AttackCombination combination)
        {
            return _combinationList.TryGet(_curCombinationIndex, out combination);
        }

        #endregion

        #region ProcessCombo

        public void DoUpdate(bool keepComboOnEnd)
        {
            ProcessTrigger();

            _attackBehaviorTimelinePlayController.Evaluate();

            if (_isProcessingCombo && _attackBehaviorTimelinePlayController.IsEnd)
            {
                if (!keepComboOnEnd)
                    ResetCombo();
                _isProcessingCombo = false;
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
            _subTrigger = true;
        }

        public bool HaveTrigger()
        {
            return _mainTrigger || _subTrigger;
        }

        public void ResetTrigger()
        {
            _mainTrigger = false;
            _subTrigger = false;
        }

        private void ProcessTrigger()
        {
            if (TryCurCombination(out var attackCombination))
            {
                if (_mainTrigger)
                    TriggerAttack(attackCombination.MainAttackBehaviorList);
                else if (_subTrigger)
                    TriggerAttack(attackCombination.SubAttackBehaviorList);
            }
            ResetTrigger();
        }

        private void TriggerAttack(List<AttackBehaviorInfo> infoList)
        {
            if (!_attackBehaviorTimelinePlayController.CanNext)
                return;

            if (infoList.Count == 0)
                return;

            _currentComboIndex++;
            if (!infoList.TryGet(_currentComboIndex, out var nextBehavior))
                return;

            _attackBehaviorTimelinePlayController.End();
            _attackBehaviorTimelinePlayController.Play(nextBehavior.TimelineAsset, 1f);
            _isProcessingCombo = true;
        }

        #endregion

        #region Reset

        public void Reset()
        {
            ResetTrigger();
            ResetCombo();
        }

        public void ResetCombo()
        {
            _currentComboIndex = -1;
        }

        #endregion

        #region ITimelineListener

        public void OnPlayTrack(ITrackRuntimeAsset trackAsset, float speedRate)
        {
            if (trackAsset is AttackClipRuntimeTrack attackTrack)
            {
                _playableClipController.Attack(attackTrack.Name, speedRate);
            }
            else if (trackAsset is AttackCollisionRuntimeTrack collisionRuntimeTrack)
            {
                CollisionAreaManager.CreateCollisionArea(
                    GameMainSystem.GetCollisionAreaSetupData(
                        _attackRefSetting,
                        collisionRuntimeTrack,
                        _fakeCharacterTriggerInfo,
                        speedRate));
            }
        }

        public void OnEndTrack(ITrackRuntimeAsset trackAsset, float speedRate)
        {

        }

        #endregion
    }
}
