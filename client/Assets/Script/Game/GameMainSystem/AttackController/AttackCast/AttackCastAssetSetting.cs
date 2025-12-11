using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Attack
{
    [CreateAssetMenu(fileName = "AttackCastAssetSetting", menuName = "GameMainModule/Attack/AttackCastAssetSetting")]
    public class AttackCastAssetSetting : ScriptableObject
    {
        public List<AttackCastTimelineAsset> AttackCastTimelineAssetList = new List<AttackCastTimelineAsset>();
        private Dictionary<int, AttackCastTimelineRuntimeAsset> _idToTimelineRuntimeAssetDic =
            new Dictionary<int, AttackCastTimelineRuntimeAsset>();

        public bool TryGetRuntimeAsset(int id, out AttackCastTimelineRuntimeAsset result)
        {
            if (_idToTimelineRuntimeAssetDic.TryGetValue(id, out result))
                return true;

            for (int i = 0; i < AttackCastTimelineAssetList.Count; i++)
            {
                if (AttackCastTimelineAssetList[i].Id == id)
                {
                    var runtimeAsset = new AttackCastTimelineRuntimeAsset(AttackCastTimelineAssetList[i]);
                    _idToTimelineRuntimeAssetDic.Add(runtimeAsset.Id, runtimeAsset);
                    result = runtimeAsset;
                    return true;
                }
            }
            return false;
        }
    }
}