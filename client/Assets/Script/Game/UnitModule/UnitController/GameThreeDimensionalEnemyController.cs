using AssetModule;
using GameMainModule;
using GameMainModule.Animation;
using GameMainModule.Attack;
using GameSystem;
using UnitModule.Movement;
using UnityEngine;
using Utility;

namespace UnitModule
{
    public class GameThreeDimensionalEnemyController : IUpdateTarget
    {
        public EnemyUnit Unit { get; private set; }

        public EnemyData Data => Unit.Data;

        [SerializeField]
        private StateMachine<EnemyState, GameThreeDimensionalEnemyState> _stateMachine =
            new StateMachine<EnemyState, GameThreeDimensionalEnemyState>();
        private GameEnemyStateContext _context = new GameEnemyStateContext();

        private TargetMovementData _movementData = new TargetMovementData();
        private EnemyAttackController _attackController = new EnemyAttackController();
        private CharacterPlayableClipController _playableClipController = new CharacterPlayableClipController();

        private EnemyManager _manager;

        public GameThreeDimensionalEnemyController(EnemyManager manager)
        {
            _manager = manager;

            _context.Manager = _manager;
            _context.MovementData = _movementData;
            _context.AttackController = _attackController;
            _context.PlayableClipController = _playableClipController;

            _stateMachine.AddState(EnemyState.Idle, new IdleState(_context));
            _stateMachine.AddState(EnemyState.Trace, new TraceState(_context));
            _stateMachine.AddState(EnemyState.Fall, new FallState(_context));
            _stateMachine.AddState(EnemyState.Attack, new AttackState(_context));

            _stateMachine.AddTransition(EnemyState.Idle, EnemyState.Trace, () => _context.TraceTarget);
            _stateMachine.AddTransition(EnemyState.Idle, EnemyState.Fall, () => !_movementData.IsGround);

            _stateMachine.AddTransition(EnemyState.Trace, EnemyState.Idle, () => !_context.TraceTarget);
            _stateMachine.AddTransition(EnemyState.Trace, EnemyState.Fall, () => !_movementData.IsGround);

            _stateMachine.AddTransition(EnemyState.Fall, EnemyState.Idle, () => _movementData.IsGround);

            _stateMachine.AddTransition(EnemyState.Idle, EnemyState.Attack, () => _context.CloseTarget);
            _stateMachine.AddTransition(EnemyState.Trace, EnemyState.Attack, () => _context.CloseTarget);
            _stateMachine.AddTransition(EnemyState.Attack, EnemyState.Idle, () => _attackController.IsEnd);
        }

        public void Init(EnemyUnit unit)
        {
            Unit = unit;
            if (Unit.HaveAvatar)
            {
                _movementData.Init(unit.UnitMovementSetting, GameMainSystem.MovementSetting);
                _playableClipController.Init(
                    "Enemy",
                    unit.UnitMovementSetting.Animator,
                    AssetSystem.LoadAsset<CharacterAnimationSetting>("Setting/CharacterAnimationSetting/PrototypeCharacter"));
                _attackController.Init(unit.AttackRefSetting, _playableClipController);
            }
            _context.UnitId = Unit.Id;
            _movementData.Speed = 5f;
            _stateMachine.SetState(EnemyState.Idle, true);
        }

        public void Clear()
        {
            Unit = null;
            _movementData.Clear();
            _attackController.Clear();
            _playableClipController.Clear();
        }

        #region IUpdateTarget

        public void DoUpdate()
        {
            _stateMachine.Update();
            _playableClipController.Update();
        }

        public void DoFixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }

        public void DoLateUpdate()
        {
            //do nothing
        }

        public void DoOnGUI()
        {
            //do nothing
        }

        public void DoDrawGizmos()
        {
            _movementData.DrawDebugGizmos();
        }

        #endregion
    }
}