using System.Collections.Generic;
using UnityEngine;

namespace UnitModule
{
    public class UnitAvatarSetting : MonoBehaviour
    {
        /// <summary>
        ///
        /// </summary>
        public Transform RootTransform;
        /// <summary>
        /// 單位人物Transform
        /// </summary>
        public Transform AvatarTransform;
        /// <summary>
        /// Animator
        /// </summary>
        public Animator Animator;
        /// <summary>
        /// Collider列表
        /// </summary>
        public List<UnitCollider> UnitColliderList = new List<UnitCollider>();
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
    }
}
