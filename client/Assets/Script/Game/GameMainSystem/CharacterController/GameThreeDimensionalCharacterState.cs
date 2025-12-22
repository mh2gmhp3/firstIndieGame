using GameMainModule.Animation;
using GameMainModule.Attack;
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
        public CharacterPlayableClipController PlayableClipController;

        public GameCharacterStateContext(
            MovementData movementData,
            CharacterAttackController attackController,
            CharacterPlayableClipController playableClipController)
        {
            MovementData = movementData;
            AttackController = attackController;
            PlayableClipController = playableClipController;
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
        protected CharacterPlayableClipController _playableClipController;

        protected GameCharacterState(GameCharacterStateContext context)
        {
            _context = context;
            _movementData = context.MovementData;
            _attackController = context.AttackController;
            _playableClipController = context.PlayableClipController;
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
            //先處理部分因為沒有進Land導致沒有重置問題
            _movementData.JumpData.ResetJump();

            _playableClipController.Idle();
            _movementData.SpeedRate = MovementData.SlowSpeedRate;
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.RotateCharacterAvatarWithSlope(_movementData);
        }
    }

    public class WalkState : GameCharacterState
    {
        private bool _isTrot = false;

        public WalkState(GameCharacterStateContext context) : base(context)
        {
        }

        public override void DoEnter(CharacterState previousState)
        {
            _isTrot = IsTrot(_movementData.MoveAxis);
            OnIsTrotChanged(_isTrot);
        }

        public override void OnUpdate()
        {
            var curIsTrot = IsTrot(_movementData.MoveAxis);
            if (curIsTrot != _isTrot)
            {
                _isTrot = curIsTrot;
                OnIsTrotChanged(_isTrot);
            }
            UpdateDirection(curIsTrot);
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.Movement(_movementData);
        }

        private bool IsTrot(Vector3 moveAxis)
        {
            return moveAxis.sqrMagnitude > 0.5f;
        }

        private void OnIsTrotChanged(bool isTrot)
        {
            var direction = _movementData.GetForwardAndRotateTransDirection();
            if (isTrot)
            {
                _movementData.SpeedRate = MovementData.MidSpeedRate;
                _playableClipController.Trot(direction);
            }
            else
            {
                _movementData.SpeedRate = MovementData.SlowSpeedRate;
                _playableClipController.Walk(direction);
            }
        }

        private void UpdateDirection(bool isTrot)
        {
            var direction = _movementData.GetForwardAndRotateTransDirection();
            if (isTrot)
            {
                _playableClipController.TrotDirection(direction);
            }
            else
            {
                _playableClipController.WalkDirection(direction);
            }
        }
    }

    public class RunState : GameCharacterState
    {
        public RunState(GameCharacterStateContext context) : base(context)
        {
        }

        public override void DoEnter(CharacterState previousState)
        {
            _playableClipController.Run(_movementData.GetForwardAndRotateTransDirection());
            _movementData.SpeedRate = MovementData.FastSpeedRate;
        }

        public override void OnUpdate()
        {
            _playableClipController.RunDirection(_movementData.GetForwardAndRotateTransDirection());
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
            _playableClipController.Jump();
        }

        public override void OnUpdate()
        {
            if (_movementData.JumpData.JumpTrigger && _movementData.JumpData.CanJump())
            {
                _movementData.JumpData.StartJump();
                _playableClipController.Jump();
            }
        }

        public override void OnFixedUpdate()
        {
            _movementData.JumpData.JumpEnd = !ThreeDimensionalMovementUtility.Jumping(_movementData);
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
            _playableClipController.Fall();
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
            _attackController.ResetCombo();
            _playableClipController.Landing();
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.RotateCharacterAvatarWithSlope(_movementData);
        }
    }

    public class DashState : GameCharacterState
    {
        public DashState(GameCharacterStateContext context) : base(context)
        {
        }

        public override void DoEnter(CharacterState previousState)
        {
            _movementData.DashData.StartDash();
            _playableClipController.Dash();
            ThreeDimensionalMovementUtility.ResetRigibody(_movementData);
            _movementData.DisableResetVelocity = true;
            ThreeDimensionalMovementUtility.Dash(_movementData);
        }

        public override void DoExit(CharacterState nextState)
        {
            _movementData.DisableResetVelocity = false;
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.RotateCharacterAvatarWithSlope(_movementData);
        }
    }

    public class AttackState : GameCharacterState
    {
        public AttackState(GameCharacterStateContext context) : base(context)
        {

        }

        private bool KeepCombo()
        {
            return !_movementData.IsGround;
        }

        public override bool CanEnter()
        {
            if (!KeepCombo())
                return true;
            return !_attackController.IsMaxCombo;
        }

        public override void DoEnter(CharacterState previousState)
        {
            if (!KeepCombo())
            {
                _attackController.ResetCombo();
            }
            _attackController.SetWeaponActive(true);
        }

        public override void DoExit(CharacterState nextState)
        {
            if (!KeepCombo())
            {
                _attackController.ResetCombo();
            }
            _attackController.SetWeaponActive(false);
        }

        public override void OnUpdate()
        {
            _attackController.DoUpdate(KeepCombo());
        }

        public override void OnFixedUpdate()
        {
            _attackController.DoFixedUpdate();
        }
    }
}
