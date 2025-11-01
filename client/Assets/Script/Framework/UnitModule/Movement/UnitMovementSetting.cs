using System;
using UnityEngine;

namespace UnitModule.Movement
{
    public class UnitMovementSetting
    {
        private Unit _unit;
        public UnitMovementSetting(Unit unit)
        {
            _unit = unit;
        }

        /// <summary>
        /// 移動用的Transform Root
        /// </summary>
        public Transform RootTransform => _unit.UnitSetting.RootTransform;
        /// <summary>
        /// 旋轉用的Transform
        /// </summary>
        public Transform RotateTransform => _unit.UnitSetting.RotateTransform;

        /// <summary>
        /// 單位人物Transform
        /// </summary>
        public Transform AvatarTransform => _unit.UnitAvatarSetting.AvatarTransform;

        /// <summary>
        /// Animator
        /// </summary>
        public Animator Animator => _unit.UnitAvatarSetting.Animator;
        /// <summary>
        /// Rigidbody
        /// </summary>
        public Rigidbody Rigidbody => _unit.UnitSetting.Rigidbody;

        /// <summary>
        /// 地面射線 以RootTransform為基準
        /// </summary>
        public Vector3 GroundRaycastStartPositionWithRootTransform => _unit.UnitAvatarSetting.GroundRaycastStartPositionWithRootTransform;
        /// <summary>
        /// 地面射線長度
        /// </summary>
        public float GroundRaycastDistance => _unit.UnitAvatarSetting.GroundRaycastDistance;
        /// <summary>
        /// 地面射線半徑
        /// </summary>
        public float GroundRaycastRadius => _unit.UnitAvatarSetting.GroundRaycastRadius;
        /// <summary>
        /// 斜坡射線長度
        /// </summary>
        public float SlopeRaycastDistance => _unit.UnitAvatarSetting.SlopeRaycastDistance;
        /// <summary>
        /// 斜坡射線半徑
        /// </summary>
        public float SlopeRaycastRadius => _unit.UnitAvatarSetting.SlopeRaycastRadius;

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
