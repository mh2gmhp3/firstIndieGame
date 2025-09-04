using Logging;
using System.Collections.Generic;
using Utility;
using static UnitModule.Movement.ThreeDimensionalMovement;
using static UnitModule.Movement.ThreeDimensionalMovementUtility;

namespace UnitModule.Movement
{
    public partial class ThreeDimensionalMovement
    {
        private SimpleStateMachine<MovementState, State> _stateMachine = new SimpleStateMachine<MovementState, State>();
        private StateCommonData _commonData = null;

        public bool IsGround
        {
            get
            {
                if (_commonData == null)
                    return false;

                return _commonData.MovementData.IsGround;
            }
        }

        private void InitState(MovementData movementData)
        {
            _commonData = new StateCommonData(this, movementData);
            _stateMachine.AddState(MovementState.Idle, new IdleState(_commonData));
            _stateMachine.AddState(MovementState.Walk, new WalkState(_commonData));
            _stateMachine.AddState(MovementState.Run, new WalkState(_commonData));
            _stateMachine.AddState(MovementState.Jump, new JumpState(_commonData));
            _stateMachine.AddState(MovementState.Fall, new FallState(_commonData));
            _stateMachine.AddState(MovementState.Land, new LandState(_commonData));
            _stateMachine.AddState(MovementState.Block, new BlockState(_commonData));
            _stateMachine.SetStateChangeEvent(NotifyStateChanged);
            _stateMachine.SetState(MovementState.Idle);
        }

        public void SetState(MovementState state, bool force = false)
        {
            _stateMachine.SetState(state, force);
        }

        public void CancelBlockState()
        {
            _commonData.IsBlock = false;
        }

        private void NotifyStateChanged(MovementState oriState, MovementState newState)
        {
            var list = _observerController.ObserverList;
            for (int i = 0; i < list.Count; i++)
            {
                var observer = list[i];
                if (observer != null)
                {
                    observer.OnStateChanged((int)oriState, (int)newState);
                }
            }
        }
    }
}
