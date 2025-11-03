using System;
using UnityEngine;
using static UnitModule.UnitAvatarManager;

namespace UnitModule.Movement
{
    public interface IUnitMovementSetting
    {
        /// <summary>
        /// 移動用的Transform Root
        /// </summary>
        public Transform RootTransform { get; }
        /// <summary>
        /// 旋轉用的Transform
        /// </summary>
        public Transform RotateTransform { get; }

        /// <summary>
        /// 單位人物Transform
        /// </summary>
        public Transform AvatarTransform { get; }

        /// <summary>
        /// Animator
        /// </summary>
        public Animator Animator { get; }
        /// <summary>
        /// Rigidbody
        /// </summary>
        public Rigidbody Rigidbody { get; }

        /// <summary>
        /// 地面射線 以RootTransform為基準
        /// </summary>
        public Vector3 GroundRaycastStartPositionWithRootTransform { get; }
        /// <summary>
        /// 地面射線長度
        /// </summary>
        public float GroundRaycastDistance { get; }
        /// <summary>
        /// 地面射線半徑
        /// </summary>
        public float GroundRaycastRadius { get; }
        /// <summary>
        /// 斜坡射線長度
        /// </summary>
        public float SlopeRaycastDistance { get; }
        /// <summary>
        /// 斜坡射線半徑
        /// </summary>
        public float SlopeRaycastRadius { get; }
    }

    public static class IUnitMovementSettingExtension
    {
        public static bool IsValid(this IUnitMovementSetting unitMovementSetting)
        {
            if (unitMovementSetting == null)
                return false;

            if (unitMovementSetting.RootTransform == null)
                return false;
            if (unitMovementSetting.RotateTransform == null)
                return false;
            if (unitMovementSetting.AvatarTransform == null)
                return false;
            if (unitMovementSetting.Animator == null)
                return false;
            if (unitMovementSetting.Rigidbody == null)
                return false;

            return true;
        }

        public static Vector3 GetGroundRaycastWorldPoint(this IUnitMovementSetting unitMovementSetting)
        {
            if (unitMovementSetting == null)
                return Vector3.zero;

            return unitMovementSetting.RootTransform.position +
                unitMovementSetting.GroundRaycastStartPositionWithRootTransform;
        }
    }
}
