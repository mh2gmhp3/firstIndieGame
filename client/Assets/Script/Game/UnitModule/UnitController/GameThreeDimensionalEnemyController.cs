using GameMainModule;
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

        public GameThreeDimensionalEnemyController()
        {
            _context.MovementData = _movementData;
            _stateMachine.AddState(EnemyState.Idle, new IdleState(_context));
            _stateMachine.AddState(EnemyState.Trace, new TraceState(_context));

            _stateMachine.AddTransition(EnemyState.Idle, EnemyState.Trace, () => _context.TraceTarget);
            _stateMachine.AddTransition(EnemyState.Trace, EnemyState.Idle, () => !_context.TraceTarget);
        }

        public void Init(EnemyUnit unit)
        {
            Unit = unit;
            if (Unit.HaveAvatar)
                _movementData.Init(unit.UnitMovementSetting, GameMainSystem.MovementSetting);
            _movementData.Speed = 5f;
            _stateMachine.SetState(EnemyState.Idle, true);
        }

        public void Clear()
        {
            Unit = null;
            _movementData.Clear();
        }

        #region IUpdateTarget

        public void DoUpdate()
        {
            _stateMachine.Update();
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
            //do nothing
        }

        #endregion
    }
}