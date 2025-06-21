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

    /// <summary>
    /// 單位使用的資料
    /// </summary>
    [Serializable]
    public class UnitData
    {
        /// <summary>
        /// 移動用設定
        /// </summary>
        public UnitMovementSetting MovementSetting = new UnitMovementSetting();

        /// <summary>
        /// 相機觀看中心點的Transform
        /// </summary>
        public Transform CameraLookupCenterTransform;
        /// <summary>
        /// Collider列表
        /// </summary>
        public List<UnitCollider> UnitColliderList = new List<UnitCollider>();
    }
}
