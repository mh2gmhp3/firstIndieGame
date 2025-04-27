using System;
using System.Collections.Generic;
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

    /// <summary>
    /// 單位使用的資料
    /// </summary>
    [Serializable]
    public class UnitData
    {
        /// <summary>
        /// Transform
        /// </summary>
        public Transform Transform;
        /// <summary>
        /// Animator
        /// </summary>
        public Animator Animator;
        /// <summary>
        /// Rigidbody
        /// </summary>
        public Rigidbody Rigidbody;
        /// <summary>
        /// Collider列表
        /// </summary>
        public List<UnitCollider> UnitColliderList = new List<UnitCollider>();
    }
}
