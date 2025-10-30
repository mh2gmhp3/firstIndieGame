using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule.Movement
{
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
        /// 單位人物Transform
        /// </summary>
        public Transform AvatarTransform;

        /// <summary>
        /// Animator
        /// </summary>
        public Animator Animator;
        /// <summary>
        /// Rigidbody
        /// </summary>
        public Rigidbody Rigidbody;

        /// <summary>
        /// 地面射線 以RootTransform為基準
        /// </summary>
        [Header("地面射線 以RootTransform為基準")]
        public Vector3 GroundRaycastStartPositionWithRootTransform;
        /// <summary>
        /// 地面射線長度
        /// </summary>
        [Header("地面射線")]
        public float GroundRaycastDistance;
        /// <summary>
        /// 地面射線半徑
        /// </summary>
        [Header("地面射線半徑")]
        public float GroundRaycastRadius;
        /// <summary>
        /// 斜坡射線長度
        /// </summary>
        [Header("斜坡射線")]
        public float SlopeRaycastDistance;
        /// <summary>
        /// 斜坡射線半徑
        /// </summary>
        [Header("斜坡射線半徑")]
        public float SlopeRaycastRadius;

        public bool IsValid()
        {
            if (RootTransform == null)
                return false;
            if (RotateTransform == null)
                return false;
            if (AvatarTransform == null)
                return false;
            if (Animator == null)
                return false;
            if (Rigidbody == null)
                return false;

            return true;
        }

        public Vector3 GetGroundRaycastWorldPoint()
        {
            return RootTransform.position + GroundRaycastStartPositionWithRootTransform;
        }
    }
}
