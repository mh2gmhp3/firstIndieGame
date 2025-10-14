using DataModule;
using Extension;
using FormModule;
using GameMainModule;
using Logging;
using System;
using System.Collections.Generic;
using static UIModule.Game.Widget_Item;

namespace UIModule.Game
{
    public partial class Widget_Page_WeaponEdit : UIWidget
    {
        public class UIAttackBehaviorEditDataContainer : IUIData, IScrollerControllerDataGetter
        {
            public List<UIAttackBehaviorEditData> AttackBehaviorEditDataList = new List<UIAttackBehaviorEditData>();

            int IScrollerControllerDataGetter.GetCellCount()
            {
                return AttackBehaviorEditDataList.Count;
            }

            string IScrollerControllerDataGetter.GetCellIdentity(int cellIndex)
            {
                return string.Empty;
            }

            IUIData IScrollerControllerDataGetter.GetUIData(int cellIndex, int widgetIndex)
            {
                if (cellIndex < 0 || cellIndex >= AttackBehaviorEditDataList.Count)
                {
                    return null;
                }

                return AttackBehaviorEditDataList[cellIndex];
            }
        }

        private EmptyData _emptyData = new EmptyData();
        private UICharacterData _characterData;
        private int _curEditWeaponIndex = -1;

        private UIAttackBehaviorEditDataContainer _editDataContainer = new UIAttackBehaviorEditDataContainer();

        protected override void DoSetData()
        {
            _curEditWeaponIndex = -1;
            OnUIDataNotify(default);
        }

        protected override void OnUIDataNotify(IUIDataNotifyInfo notifyInfo)
        {
            if (_uiData is UICharacterData characterData)
            {
                _characterData = characterData;
                SetWeapon(characterData.WeaponRefItemIdList);
                SetCurEditWeaponIndex(_curEditWeaponIndex);
            }
        }

        private void SetWeapon(List<int> weaponRefItemIdList)
        {
            var weaponCount = weaponRefItemIdList.Count;
            for (int i = 0; i < Widget_Item_Weapons.Count; i++)
            {
                var widget = Widget_Item_Weapons[i];
                if (i >= weaponCount)
                {
                    widget.SetData(_emptyData);
                    continue;
                }

                var itemId = weaponRefItemIdList[i];
                if (!GameMainSystem.TryGetItemData(itemId, out var itemData))
                {
                    widget.SetData(_emptyData);
                    continue;
                }

                widget.SetData(itemData);
            }
        }

        private void SetCurEditWeaponIndex(int index)
        {
            if (_characterData == null)
            {
                Log.LogError("Widget_Page_WeaponEdit SetCurEditWeaponIndex Error, _characterData is null");
                return;
            }

            if (index == -1)
            {
                //-1從第一個開始看 不是空的就直接使用
                for (int i = 0; i< _characterData.WeaponRefItemIdList.Count; i++)
                {
                    if (_characterData.WeaponRefItemIdList[i] != CommonDefine.EmptyWeaponId)
                    {
                        //替換Index
                        index = i;
                        break;
                    }
                }
            }

            if (index < 0 || index >= _characterData.WeaponRefItemIdList.Count)
            {
                GameObject_AttackBehavior_Root.SetActive(false);
                return;
            }

            var weaponRefItemId = _characterData.WeaponRefItemIdList[index];
            if (weaponRefItemId == CommonDefine.EmptyWeaponId)
            {
                GameObject_AttackBehavior_Root.SetActive(false);
                return;
            }

            if (!GameMainSystem.TryGetUIWeaponData(weaponRefItemId, out var weaponData))
            {
                GameObject_AttackBehavior_Root.SetActive(false);
                Log.LogError($"Widget_Page_WeaponEdit SetCurEditWeaponIndex Error, weaponData not exist, WeaponRefItemId:{weaponRefItemId}");
                return;
            }

            if (!FormSystem.Table.WeaponTable.TryGetData(weaponData.SettingId, out var weaponRow))
            {
                GameObject_AttackBehavior_Root.SetActive(false);
                Log.LogError($"Widget_Page_WeaponEdit SetCurEditWeaponIndex Error, WeaponRow not exist, " +
                    $"WeaponRefItemId:{weaponRefItemId}, " +
                    $"WeaponSettingId:{weaponData.SettingId}");
                return;
            }

            GameObject_AttackBehavior_Root.SetActive(true);
            _editDataContainer.AttackBehaviorEditDataList.Clear();
            var haveSetup = _characterData.WeaponBehaviorSetupDic.TryGetValue(weaponRefItemId, out var setupData);
            var createBehaviorCount = Math.Max(
                weaponRow.BaseBehaviorCount,
                haveSetup ? setupData.AttackBehaviorRefItemIdList.Count : 0);
            for (int i = 0; i < createBehaviorCount; i++)
            {
                var editData = new UIAttackBehaviorEditData(i, CommonDefine.EmptyAttackBehaviorId, weaponRefItemId);
                if (haveSetup)
                {
                    if (setupData.AttackBehaviorRefItemIdList.TryGet(i, out var behaviorRefItemId))
                        editData.RefItemId = behaviorRefItemId;
                }
                _editDataContainer.AttackBehaviorEditDataList.Add(editData);
            }
            _curEditWeaponIndex = index;
            SimpleScrollerController_AttackBehavior.SetDataGetter(_editDataContainer);
        }
    }
}
