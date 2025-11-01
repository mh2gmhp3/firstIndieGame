using CollisionModule;
using UnitModule;
using Logging;
using System.Collections.Generic;

namespace Utility
{
    public static class UnitUtility
    {
        /// <summary>
        /// 獲取註冊用碰撞資料
        /// <para>註冊用資料:<see cref="CollisionAreaManager.RegisterColliderData"/></para>
        /// <para>註冊使用方法:<see cref="CollisionAreaManager.RegisterCollider(CollisionAreaManager.RegisterColliderData)"/></para>
        /// </summary>
        /// <param name="unitColliderList"></param>
        /// <returns></returns>
        public static CollisionAreaManager.RegisterColliderData GetColliderData(this List<UnitCollider> unitColliderList)
        {
            var result = new CollisionAreaManager.RegisterColliderData();
            if (unitColliderList == null)
            {
                Log.LogError("UnitUtility.RegisterColliderData Error: unitColliderList is null");
                return result;
            }

            for (int i = 0; i < unitColliderList.Count; i++)
            {
                var unitCollider = unitColliderList[i];
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
