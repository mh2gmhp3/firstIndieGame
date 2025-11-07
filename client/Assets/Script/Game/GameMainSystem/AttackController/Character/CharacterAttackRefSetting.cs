using System.Collections;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;

namespace GameMainModule.Attack
{
    public class CharacterAttackRefSetting : IAttackRefSetting
    {
        public IUnitMovementSetting _unitMovementSetting;

        public CharacterAttackRefSetting(IUnitMovementSetting unitMovementSetting)
        {
            _unitMovementSetting = unitMovementSetting;
        }

        public bool TryGetAttackCastPoint(int id, out Vector3 worldPoint, out Vector3 direction)
        {
            worldPoint = _unitMovementSetting.RootTransform.position + new Vector3(0f, 0.5f, 0f);
            direction = _unitMovementSetting.RotateTransform.forward;
            return true;
        }
    }
}
