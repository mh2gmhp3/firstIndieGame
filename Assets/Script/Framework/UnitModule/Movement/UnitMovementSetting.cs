using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule.Movement
{
    [Serializable]
    public class UnitMovementRaycastSetting
    {
        public Vector3 Position;
        public Vector3 Direction;
        public float Distance;
    }

    /// <summary>
    /// 單位移動使用資料設定
    /// </summary>
    [Serializable]
    public class UnitMovementSetting
    {
        /// <summary>
        /// 移動用的Transform Root
        /// </summary>
        public Transform RootTransform;
        /// <summary>
        /// 旋轉用的Transform
        /// </summary>
        public Transform RotateTransform;

        /// <summary>
        /// Animator
        /// </summary>
        public Animator Animator;
        /// <summary>
        /// Rigidbody
        /// </summary>
        public Rigidbody Rigidbody;

        [Header("地面射線偏移 以RootTransform為基準")]
        public List<UnitMovementRaycastSetting> GroundRaycastSetting = new List<UnitMovementRaycastSetting>();
    }
}
