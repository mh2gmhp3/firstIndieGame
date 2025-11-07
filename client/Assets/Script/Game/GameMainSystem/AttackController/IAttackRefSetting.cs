using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Attack
{
    public interface IAttackRefSetting
    {
        /// <summary>
        /// 對應傳入Id自行回傳釋放位置
        /// </summary>
        /// <param name="id"></param>
        /// <param name="worldPoint"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        bool TryGetAttackCastPoint(int id, out Vector3 worldPoint, out Vector3 direction);

    }
}
