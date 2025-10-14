using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIModule.Game
{
    public class UIAttackBehaviorEditData : IUIData
    {
        public int Index;
        public int RefItemId;
        public int RefWeaponItemId;

        public UIAttackBehaviorEditData(int index, int refItemId, int refWeaponItemId)
        {
            Index = index;
            RefItemId = refItemId;
            RefWeaponItemId = refWeaponItemId;
        }
    }
}
