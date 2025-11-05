using AnimationModule;
using GameMainModule;
using GameMainModule.Animation;
using GameMainModule.Attack;
using System.Collections;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;
using Utility;
using static UnitModule.Movement.ThreeDimensionalMovementUtility;

namespace UnitModule
{
    public enum EnemyState
    {
        Idle,
        Track,
        Attack,
    }

    public class GameEnemyStateContext
    {
        public TargetMovementData MovementData;
        public bool TrackTarget = false;
        public float TrackDistance = 2f;
    }

    public class GameThreeDimensionalEnemyState : State<EnemyState>
    {
        protected GameEnemyStateContext _context;
        protected TargetMovementData _movementData;

        protected GameThreeDimensionalEnemyState(GameEnemyStateContext context)
        {
            _context = context;
            _movementData = context.MovementData;
        }

        public sealed override void DoUpdate()
        {
            ThreeDimensionalMovementUtility.UpdateState(_movementData);
            OnUpdate();
        }

        public sealed override void DoFixedUpdate()
        {
            ThreeDimensionalMovementUtility.ResetRigibody(_movementData);
            OnFixedUpdate();
        }

        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
    }

    public class IdleState : GameThreeDimensionalEnemyState
    {
        public IdleState(GameEnemyStateContext context) : base(context)
        {
        }

        public override void OnUpdate()
        {
            var targetPos = GameMainSystem.GetCharacterPosition();
            if ((_movementData.UnitMovementSetting.RootTransform.position - targetPos).sqrMagnitude >
                _context.TrackDistance * _context.TrackDistance)
            {
                _movementData.TargetPosition =  targetPos;
                _context.TrackTarget = true;
            }
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.RotateCharacterAvatarWithSlope(_movementData);
        }
    }

    public class TrackState : GameThreeDimensionalEnemyState
    {
        public TrackState(GameEnemyStateContext context) : base(context)
        {
        }

        public override void OnUpdate()
        {
            var targetPos = GameMainSystem.GetCharacterPosition();
            if ((_movementData.UnitMovementSetting.RootTransform.position - targetPos).sqrMagnitude >
                _context.TrackDistance * _context.TrackDistance)
            {
                _movementData.TargetPosition = targetPos;
            }
            else
            {
                _context.TrackTarget = false;
            }
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.Movement(_movementData);
        }
    }
}
