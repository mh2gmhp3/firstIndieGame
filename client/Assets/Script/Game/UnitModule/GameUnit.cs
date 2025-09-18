using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule
{
    /// <summary>
    /// 角色單位
    /// </summary>
    public class GameUnit : MonoBehaviour
    {
        [SerializeField]
        private UnitData _unitData;

        public UnitData UnitData => _unitData;
    }
}
