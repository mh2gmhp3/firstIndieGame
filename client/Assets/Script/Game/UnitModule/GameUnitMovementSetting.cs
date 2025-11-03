using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitModule.UnitAvatarManager;

namespace UnitModule.Movement
{
    public class GameUnitMovementSetting : IUnitMovementSetting
    {
        private UnitAvatarInstance _avatarInstance;
        public GameUnitMovementSetting(UnitAvatarInstance avatarInstance)
        {
            _avatarInstance = avatarInstance;
        }

        public Transform RootTransform => _avatarInstance.UnitSetting.RootTransform;
        public Transform RotateTransform => _avatarInstance.UnitSetting.RotateTransform;
        public Transform AvatarTransform => _avatarInstance.UnitAvatarSetting.AvatarTransform;
        public Animator Animator => _avatarInstance.UnitAvatarSetting.Animator;
        public Rigidbody Rigidbody => _avatarInstance.UnitSetting.Rigidbody;
        public Vector3 GroundRaycastStartPositionWithRootTransform => _avatarInstance.UnitAvatarSetting.GroundRaycastStartPositionWithRootTransform;
        public float GroundRaycastDistance => _avatarInstance.UnitAvatarSetting.GroundRaycastDistance;
        public float GroundRaycastRadius => _avatarInstance.UnitAvatarSetting.GroundRaycastRadius;
        public float SlopeRaycastDistance => _avatarInstance.UnitAvatarSetting.SlopeRaycastDistance;
        public float SlopeRaycastRadius => _avatarInstance.UnitAvatarSetting.SlopeRaycastRadius;
    }
}
