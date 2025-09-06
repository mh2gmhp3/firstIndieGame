using GameMainModule.Attack;
using System;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;
using Utility;
using static UnitModule.Movement.ThreeDimensionalMovementUtility;

namespace GameMainModule
{
    [Serializable]
    public class GameThreeDimensionalCharacterController
    {
        [SerializeField]
        private StateMachine<CharacterState, GameCharacterState> _characterStateMachine =
            new StateMachine<CharacterState, GameCharacterState>();

        [SerializeField]
        private MovementData _movementData;
        [SerializeField]
        private CharacterAttackController _characterAttackController = new CharacterAttackController();
        private GameUnitAnimationController _animationController = new GameUnitAnimationController();

        private GameCharacterStateContext _characterStateContext;


        public GameThreeDimensionalCharacterController()
        {

        }

        public void InitController(UnitMovementSetting unityMovementSetting, MovementSetting movementSetting)
        {
            _movementData = new MovementData(unityMovementSetting, movementSetting);
            _characterStateContext = new GameCharacterStateContext(_movementData, _characterAttackController);

            //Animator
            _animationController.SetAnimatior(unityMovementSetting.Animator);
            _characterAttackController.AddObserver(_animationController);

            //State
            _characterStateMachine.AddState(CharacterState.Idle, new IdleState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Walk, new WalkState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Run, new WalkState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Jump, new JumpState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Fall, new FallState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Land, new LandState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Attack, new AttackState(_characterStateContext));
            _characterStateMachine.SetStateChangeEvent(OnStateChanged);
            _characterStateMachine.SetState(CharacterState.Idle, true);

            //State Transition
            //Idle
            _characterStateMachine.AddTransition(CharacterState.Idle, CharacterState.Walk,
                () => { return _movementData.IsGround && _movementData.HaveMoveInput(); });
            _characterStateMachine.AddTransition(CharacterState.Idle, CharacterState.Fall,
                () => { return !_movementData.IsGround; });
            _characterStateMachine.AddTransition(CharacterState.Idle, CharacterState.Jump,
                () => { return _movementData.JumpData.JumpTrigger; });

            //Walk
            _characterStateMachine.AddTransition(CharacterState.Walk, CharacterState.Idle,
                () => { return _movementData.IsGround && !_movementData.HaveMoveInput(); });
            _characterStateMachine.AddTransition(CharacterState.Walk, CharacterState.Fall,
                () => { return !_movementData.IsGround; });
            _characterStateMachine.AddTransition(CharacterState.Walk, CharacterState.Jump,
                () => { return _movementData.JumpData.JumpTrigger; });
            _characterStateMachine.AddTransition(CharacterState.Walk, CharacterState.Run,
                () => { return _movementData.RunTriggered; });

            //Run
            _characterStateMachine.AddTransition(CharacterState.Run, CharacterState.Idle,
                () => { return _movementData.IsGround && !_movementData.HaveMoveInput(); });
            _characterStateMachine.AddTransition(CharacterState.Run, CharacterState.Fall,
                () => { return !_movementData.IsGround; });
            _characterStateMachine.AddTransition(CharacterState.Run, CharacterState.Jump,
                () => { return _movementData.JumpData.JumpTrigger; });
            _characterStateMachine.AddTransition(CharacterState.Run, CharacterState.Walk,
                () => { return !_movementData.RunTriggered; });

            //Jump
            _characterStateMachine.AddTransition(CharacterState.Jump, CharacterState.Fall,
                () => { return _movementData.JumpData.JumpEnd; });

            //Fall
            _characterStateMachine.AddTransition(CharacterState.Fall, CharacterState.Land,
                () => { return _movementData.IsGround; });
            _characterStateMachine.AddTransition(CharacterState.Fall, CharacterState.Jump,
                () => { return _movementData.JumpData.JumpTrigger; });

            //Land
            _characterStateMachine.AddTransition(CharacterState.Land, CharacterState.Idle,
                () => { return _movementData.LandData.LandElapsedTime() >= _movementData.MovementSetting.LandStiffTime; });

            //Attack
            _characterStateMachine.AddTransition(CharacterState.Attack, CharacterState.Idle,
                () => { return !_characterAttackController.IsProcessCombo && _movementData.IsGround; });
            _characterStateMachine.AddTransition(CharacterState.Attack, CharacterState.Fall,
                () => { return !_characterAttackController.IsProcessCombo && !_movementData.IsGround; });
            //To Attack
            _characterStateMachine.AddTransition(CharacterState.Idle, CharacterState.Attack,
                () => { return _characterAttackController.HaveTrigger(); });
            _characterStateMachine.AddTransition(CharacterState.Walk, CharacterState.Attack,
                () => { return _characterAttackController.HaveTrigger(); });
            _characterStateMachine.AddTransition(CharacterState.Run, CharacterState.Attack,
                () => { return _characterAttackController.HaveTrigger(); });
            _characterStateMachine.AddTransition(CharacterState.Jump, CharacterState.Attack,
                () => { return _characterAttackController.HaveTrigger(); });
            _characterStateMachine.AddTransition(CharacterState.Fall, CharacterState.Attack,
                () => { return _characterAttackController.HaveTrigger(); });
        }

        public void DoUpdate()
        {
            _characterStateMachine.Update();
        }

        public void DoFixedUpdate()
        {
            _characterStateMachine.FixedUpdate();
        }

        public void DoOnGUI()
        {

        }

        #region Movement

        /// <summary>
        /// 設定移動輸入軸
        /// </summary>
        /// <param name="axis"></param>
        public void SetMoveAxis(Vector3 axis)
        {
            if (_movementData == null)
                return;
            _movementData.MoveAxis = axis;
        }

        /// <summary>
        /// 設定移動選轉方向
        /// </summary>
        /// <param name="quaternion"></param>
        public void SetMoveQuaternion(Quaternion quaternion)
        {
            if (_movementData == null)
                return;
            //只取Y軸
            _movementData.MoveQuaternion = new Quaternion(0, quaternion.y, 0, quaternion.w);
        }

        public void Jump()
        {
            _movementData.JumpData.JumpTrigger = true;
        }

        public void Run()
        {
            _movementData.RunTriggered = true;
        }

        #endregion

        #region Attack

        public void MainAttack()
        {
            _characterAttackController.TriggerMainAttack();
        }

        public void SubAttack()
        {
            _characterAttackController.TriggerSubAttack();
        }

        public void SetCombinationList(List<AttackCombination> combinationList)
        {
            _characterAttackController.SetCombinationList(combinationList);
        }

        public void SetNowCombination(int index)
        {
            _characterAttackController.SetNowCombination(index);
        }

        #endregion

        public void OnStateChanged(CharacterState oriState, CharacterState newState)
        {
            _animationController.OnStateChanged((int)oriState, (int)newState);
        }
    }
}
