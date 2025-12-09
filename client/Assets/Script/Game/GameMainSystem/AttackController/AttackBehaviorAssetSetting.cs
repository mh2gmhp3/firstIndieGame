using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Attack
{
    [CreateAssetMenu(fileName = "AttackBehaviorAssetSetting", menuName = "GameMainModule/Attack/AttackBehaviorAssetSetting")]
    public class AttackBehaviorAssetSetting : ScriptableObject
    {
        public List<AttackBehaviorTimelineAsset> AttackBehaviorTimelineAssetList = new List<AttackBehaviorTimelineAsset>();
        private Dictionary<int, AttackBehaviorTimelineRuntimeAsset> _idToTimelineRuntimeAssetDic =
            new Dictionary<int, AttackBehaviorTimelineRuntimeAsset>();

        public bool TryGetRuntimeAsset(int id, out AttackBehaviorTimelineRuntimeAsset result)
        {
            if (_idToTimelineRuntimeAssetDic.TryGetValue(id, out result))
                return true;

            for (int i = 0; i < AttackBehaviorTimelineAssetList.Count; i++)
            {
                if (AttackBehaviorTimelineAssetList[i].Id == id)
                {
                    var runtimeAsset = new AttackBehaviorTimelineRuntimeAsset(AttackBehaviorTimelineAssetList[i]);
                    _idToTimelineRuntimeAssetDic.Add(runtimeAsset.Id, runtimeAsset);
                    result = runtimeAsset;
                    return true;
                }
            }
            return false;
        }
    }
}
