using Utility;
using static UnitModule.Movement.ThreeDimensionalMovementUtility;

namespace UnitModule.Movement
{
    public class StateCommonData
    {
        public IMovement Controller;
        public MovementData MovementData;

        public bool IsBlock = false;

        public StateCommonData(IMovement controller, MovementData movementData)
        {
            Controller = controller;
            MovementData = movementData;
        }
    }

    public abstract class State : IStateIns<MovementState>
    {
        protected StateCommonData _commonData;
        protected IMovement _controller;
        protected MovementData _movementData;

        public State(StateCommonData commonData)
        {
            _commonData = commonData;
            _controller = commonData.Controller;
            _movementData = commonData.MovementData;
        }

        public virtual void DoUpdate() { }
        public virtual void DoFixedUpdate() { }
        public virtual void DoEnter(MovementState previousState) { }
        public virtual void DoExit(MovementState nextState) { }

        public virtual void DoInputCommand(int command) { }

        public virtual void DoGUI() { }
    }

    public class IdleState : State
    {
        public IdleState(StateCommonData commonData) : base(commonData)
        {
        }

        public override void DoUpdate()
        {
            if (_movementData.IsGround)
            {
                if (_movementData.HaveMoveInput())
                {
                    _controller.SetState(MovementState.Walk);
                }
            }
            else
            {
                _controller.SetState(MovementState.Fall);
            }
        }

        public override void DoFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
        }

        public override void DoInputCommand(int command)
        {
            if (command == (int)MovementInputCommand.Jump)
            {
                _controller.SetState(MovementState.Jump);
            }
        }
    }

    public class WalkState : State
    {
        public WalkState(StateCommonData commonData) : base(commonData)
        {
        }

        public override void DoUpdate()
        {
            if (_movementData.IsGround)
            {
                if (!_movementData.HaveMoveInput())
                {
                    _controller.SetState(MovementState.Idle);
                }
            }
            else
            {
                _controller.SetState(MovementState.Fall);
            }
        }

        public override void DoFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.Movement(_movementData);
        }

        public override void DoInputCommand(int command)
        {
            if (command == (int)MovementInputCommand.Jump)
            {
                _controller.SetState(MovementState.Jump);
            }
        }
    }

    public class RunState : State
    {
        public RunState(StateCommonData commonData) : base(commonData)
        {
        }

        public override void DoUpdate()
        {
            if (_movementData.IsGround)
            {
                if (!_movementData.HaveMoveInput())
                {
                    _controller.SetState(MovementState.Idle);
                }
            }
            else
            {
                _controller.SetState(MovementState.Fall);
            }
        }

        public override void DoFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
            ThreeDimensionalMovementUtility.Movement(_movementData);
        }

        public override void DoInputCommand(int command)
        {
            if (command == (int)MovementInputCommand.Jump)
            {
                _controller.SetState(MovementState.Jump);
            }
        }
    }

    public class JumpState : State
    {
        public JumpState(StateCommonData commonData) : base(commonData)
        {
        }

        public override void DoEnter(MovementState previousState)
        {
            if (previousState == MovementState.Block)
            {
                _controller.SetState(MovementState.Fall);
                return;
            }
            _movementData.JumpData.StartJump();
        }

        public override void DoFixedUpdate()
        {
            if (!ThreeDimensionalMovementUtility.Jump(_movementData))
            {
                _controller.SetState(MovementState.Fall);
            }
            ThreeDimensionalMovementUtility.Movement(_movementData);
        }

        public override void DoInputCommand(int command)
        {
            if (command == (int)MovementInputCommand.Jump)
            {
                //多段跳
                if (!_movementData.JumpData.CanJump())
                    return;
                _movementData.JumpData.StartJump();
            }
        }
    }

    public class FallState : State
    {
        public FallState(StateCommonData commonData) : base(commonData)
        {
        }

        public override void DoEnter(MovementState previousState)
        {
            _movementData.FallData.StartFall();
        }

        public override void DoUpdate()
        {
            if (_movementData.IsGround)
            {
                _controller.SetState(MovementState.Land);
            }
        }

        public override void DoFixedUpdate()
        {
            ThreeDimensionalMovementUtility.GravityFall(_movementData);
            ThreeDimensionalMovementUtility.Movement(_movementData);
        }

        public override void DoInputCommand(int command)
        {
            if (command == (int)MovementInputCommand.Jump)
            {
                if (!_movementData.JumpData.CanJump())
                    return;
                _controller.SetState(MovementState.Jump);
            }
        }
    }

    public class LandState : State
    {
        public LandState(StateCommonData commonData) : base(commonData)
        {
        }

        public override void DoEnter(MovementState previousState)
        {
            _movementData.JumpData.ResetJump();
            _movementData.LandData.StartLand();
        }

        public override void DoUpdate()
        {
            if (_movementData.LandData.LandElapsedTime() >= _movementData.MovementSetting.LandStiffTime)
            {
                _controller.SetState(MovementState.Idle);
            }
        }

        public override void DoFixedUpdate()
        {
            ThreeDimensionalMovementUtility.FixGroundPoint(_movementData);
        }
    }

    public class BlockState : State
    {
        private MovementState _previousState;

        public BlockState(StateCommonData commonData) : base(commonData)
        {
        }

        public override void DoEnter(MovementState previousState)
        {
            _previousState = previousState;
            _commonData.IsBlock = true;
        }

        public override void DoUpdate()
        {
            if (!_commonData.IsBlock)
            {
                _controller.SetState(_previousState);
            }
        }
    }
}
