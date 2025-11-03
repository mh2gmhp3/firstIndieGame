using System;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;

namespace UnitModule
{
    /// <summary>
    /// 單位的Collider
    /// </summary>
    [Serializable]
    public class UnitCollider
    {
        /// <summary>
        /// Collider Id
        /// </summary>
        public int Id;
        /// <summary>
        /// Collider
        /// </summary>
        public Collider Collider;
    }
}
