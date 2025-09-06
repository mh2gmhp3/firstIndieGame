using GameMainModule.Animation;
using GameMainModule.Attack;
using System.Collections;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;
using Utility;
using static UnitModule.Movement.ThreeDimensionalMovementUtility;

namespace GameMainModule
{
    public enum CharacterState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Fall,
        Land,
        Dash,
        Attack,
        KnockDown
    }

    public class GameCharacterStateContext
    {
        public MovementData MovementData;
        public CharacterAttackController AttackController;
        public AnimatorController AnimatorController;

        public GameCharacterStateContext(
            MovementData movementData,
            CharacterAttackController attackController,
            AnimatorController animatorController)
        {
            MovementData = movementData;
            AttackController = attackController;
            AnimatorController = animatorController;
        }

        public void ResetTrigger()
        {
            MovementData.ResetTrigger();
            AttackController.ResetTrigger();
        }
    }

    public abstract class GameCharacterState : State<CharacterState>
    {
        protected GameCharacterStateContext _context;
        protected MovementData _movementData;
        protected CharacterAttackController _attackController;
        protected AnimatorController _animatorController;

        protected GameCharacterState(GameCharacterStateContext context)
        {
            _context = context;
            _movementData = context.MovementData;
            _attackController = context.AttackController;
            _animatorController = context.AnimatorController;
        }

        public sealed override void DoUpdate()
        {
            ThreeDimensionalMovementUtility.UpdateState(_movementData);
            OnUpdate();
            _context.ResetTrigger();
        }

        public sealed override void DoFixedUpdate()
        {
            ThreeDimensionalMovementUtility.ResetRigibody(_movementData);
            OnFixedUpdate();
        }

        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }

        public virtual void DoInputCommand(int command) { }
    }

    public class IdleState : GameCharacterState
    {
        public IdleState(GameCharacterStateContext context) : base(context)
        {
        }

        public override void DoEnter(CharacterState previousState)
        {
            _animatorController.CrossFade("Idle");
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
        }
    }

    public class WalkState : GameCharacterState
    {
        public WalkState(GameCharacterStateContext context) : base(context)
        {
        }

        public override void DoEnter(CharacterState previousState)
        {
            _animatorController.CrossFade("Walk");
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.Movement(_movementData);
        }
    }

    public class RunState : GameCharacterState
    {
        public RunState(GameCharacterStateContext context) : base(context)
        {
        }

        public override void DoEnter(CharacterState previousState)
        {
            _animatorController.CrossFade("Run");
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.Movement(_movementData);
        }
    }

    public class JumpState : GameCharacterState
    {
        public JumpState(GameCharacterStateContext context) : base(context)
        {
        }

        public override bool CanEnter()
        {
            return _movementData.JumpData.CanJump();
        }

        public override void DoEnter(CharacterState previousState)
        {
            _movementData.JumpData.StartJump();
            _animatorController.CrossFade("Jump_s");
        }

        public override void OnUpdate()
        {
            if (_movementData.JumpData.JumpTrigger && _movementData.JumpData.CanJump())
            {
                _movementData.JumpData.StartJump();
                _animatorController.CrossFade("Jump_s");
            }
        }

        public override void OnFixedUpdate()
        {
            _movementData.JumpData.JumpEnd = !ThreeDimensionalMovementUtility.Jump(_movementData);
            ThreeDimensionalMovementUtility.Movement(_movementData);
        }
    }

    public class FallState : GameCharacterState
    {
        public FallState(GameCharacterStateContext context) : base(context)
        {
        }

        public override void DoEnter(CharacterState previousState)
        {
            _movementData.FallData.StartFall();
            _animatorController.CrossFade("Fall_d");
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.GravityFall(_movementData);
            ThreeDimensionalMovementUtility.Movement(_movementData);
        }
    }

    public class LandState : GameCharacterState
    {
        public LandState(GameCharacterStateContext context) : base(context)
        {
        }

        public override bool CanExit()
        {
            return _movementData.LandData.LandElapsedTime() >= _movementData.MovementSetting.LandStiffTime;
        }

        public override void DoEnter(CharacterState previousState)
        {
            _movementData.JumpData.ResetJump();
            _movementData.LandData.StartLand();
            _animatorController.CrossFade("Fall_e");
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
        }
    }

    public class AttackState : GameCharacterState, IAttackCombinationObserver
    {
        public AttackState(GameCharacterStateContext context) : base(context)
        {
            _attackController.AddObserver(this);
        }

        public override void DoEnter(CharacterState previousState)
        {
            //_attackController.KeepComboing(!_movementData.IsGround);
            //_attackController.ProcessTrigger();
        }

        public override void OnUpdate()
        {
            _attackController.DoUpdate();
        }

        #region IAttackCombinationObserver

        public void OnStartAttackBehavior(string behaviorName)
        {
            _animatorController.CrossFade(behaviorName);
        }

        public void OnStartComboing()
        {

        }

        public void OnEndComboing()
        {

        }

        #endregion
    }
}
