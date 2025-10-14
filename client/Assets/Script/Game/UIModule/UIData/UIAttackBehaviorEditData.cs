using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIModule.Game
{
    public class UIAttackBehaviorEditData : IUIData
    {
        public int Index;
        public int RefItemId;
        public int CurEditWeaponType;
        public int CurEditWeaponRefItemId;

        public UIAttackBehaviorEditData(int index, int refItemId, int curEditWeaponType,  int refWeaponItemId)
        {
            Index = index;
            RefItemId = refItemId;
            CurEditWeaponType = curEditWeaponType;
            CurEditWeaponRefItemId = refWeaponItemId;
        }
    }
}
