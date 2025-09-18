using CollisionModule;
using UnitModule;
using Logging;

namespace Utility
{
    public static class UnitUtility
    {
        /// <summary>
        /// 獲取註冊用碰撞資料
        /// <para>註冊用資料:<see cref="CollisionAreaManager.RegisterColliderData"/></para>
        /// <para>註冊使用方法:<see cref="CollisionAreaManager.RegisterCollider(CollisionAreaManager.RegisterColliderData)"/></para>
        /// </summary>
        /// <param name="unitData"></param>
        /// <returns></returns>
        public static CollisionAreaManager.RegisterColliderData GetColliderData(this UnitData unitData)
        {
            var result = new CollisionAreaManager.RegisterColliderData();
            if (unitData == null)
            {
                Log.LogError("UnitUtility.RegisterColliderData Error: unitData is null");
                return result;
            }

            for (int i = 0; i < unitData.UnitColliderList.Count; i++)
            {
                var unitCollider = unitData.UnitColliderList[i];
                if (result.IdToRegisterColliderDic.ContainsKey(unitCollider.Id))
                {
                    Log.LogWarning($"UnitUtility.RegisterColliderData Error: duplicate id:{unitCollider.Id}");
                    continue;
                }
                result.IdToRegisterColliderDic.Add(unitCollider.Id, unitCollider.Collider);
            }

            return result;
        }
    }
}
