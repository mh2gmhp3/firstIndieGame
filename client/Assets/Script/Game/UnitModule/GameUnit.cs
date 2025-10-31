using UnityEngine;

namespace UnitModule
{
    /// <summary>
    /// 遊戲單位
    /// </summary>
    public class GameUnit : MonoBehaviour
    {
        [SerializeField]
        private UnitData _unitData;

        public UnitData UnitData => _unitData;
    }
}
