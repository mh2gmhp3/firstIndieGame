using AnimationModule;
using GameMainModule;
using GameMainModule.Animation;
using UnitModule.Movement;
using Utility;

namespace UnitModule
{
    public enum EnemyState
    {
        Idle,
        Trace,
        Fall,
        Attack,
    }

    public class GameEnemyStateContext
    {
        public int UnitId;
        public EnemyManager Manager;
        public TargetMovementData MovementData;
        public CharacterPlayableClipController PlayableClipController;
        public bool TraceTarget = false;
        public float TrackDistance = 2f;
    }

    public class GameThreeDimensionalEnemyState : State<EnemyState>
    {
        protected GameEnemyStateContext _context;
        protected EnemyManager _manager;
        protected TargetMovementData _movementData;
        protected CharacterPlayableClipController _playableClipController;

        protected GameThreeDimensionalEnemyState(GameEnemyStateContext context)
        {
            _context = context;
            _manager = context.Manager;
            _movementData = context.MovementData;
            _playableClipController = context.PlayableClipController;
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

        public override void DoEnter(EnemyState previousState)
        {
            _playableClipController.Idle();
        }

        public override void OnUpdate()
        {
            var targetPos = GameMainSystem.GetCharacterPosition();
            if ((_movementData.UnitMovementSetting.RootTransform.position - targetPos).sqrMagnitude >
                _context.TrackDistance * _context.TrackDistance)
            {
                _movementData.TargetPosition =  targetPos;
                _context.TraceTarget = true;
            }
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.RotateCharacterAvatarWithSlope(_movementData);
        }
    }

    public class TraceState : GameThreeDimensionalEnemyState
    {
        public TraceState(GameEnemyStateContext context) : base(context)
        {
        }

        public override void DoEnter(EnemyState previousState)
        {
            _playableClipController.Trot(0);
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
                _context.TraceTarget = false;
            }
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.Movement(_movementData);
        }
    }

    public class FallState : GameThreeDimensionalEnemyState
    {
        public FallState(GameEnemyStateContext context) : base(context)
        {
        }

        public override void DoEnter(EnemyState previousState)
        {
            _movementData.FallData.StartFall();
            _playableClipController.Fall();
        }

        public override void OnFixedUpdate()
        {
            ThreeDimensionalMovementUtility.GravityFall(_movementData);
            ThreeDimensionalMovementUtility.Movement(_movementData);

            if (_movementData.GetFallElapsedTime() > 10)
            {
                _manager.AddDead(_context.UnitId);
            }
        }
    }
}
