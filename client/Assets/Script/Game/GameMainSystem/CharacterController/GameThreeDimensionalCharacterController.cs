using GameMainModule.Animation;
using GameMainModule.Attack;
using GameSystem;
using System;
using UnitModule;
using UnitModule.Movement;
using UnityEngine;
using Utility;
using static UnitModule.Movement.ThreeDimensionalMovementUtility;

namespace GameMainModule
{
    [Serializable]
    public class GameThreeDimensionalCharacterController : IUpdateTarget
    {
        public CharacterUnit Unit { get; private set; }

        [SerializeField]
        private StateMachine<CharacterState, GameCharacterState> _characterStateMachine =
            new StateMachine<CharacterState, GameCharacterState>();
        private GameCharacterStateContext _characterStateContext;

        [SerializeField]
        private MovementData _movementData;
        [SerializeField]
        private CharacterAttackController _characterAttackController = new CharacterAttackController();
        private CharacterPlayableClipController _playableClipController = new CharacterPlayableClipController();


        public CharacterAttackController AttackController => _characterAttackController;

        public GameThreeDimensionalCharacterController()
        {

        }

        public void InitController(
            CharacterUnit characterUnit,
            MovementSetting movementSetting,
            CharacterAnimationSetting characterAnimationSetting)
        {
            Unit = characterUnit;
            _movementData = new MovementData(characterUnit.UnitMovementSetting, movementSetting);
            _characterStateContext = new GameCharacterStateContext(
                _movementData,
                _characterAttackController,
                _playableClipController);

            //Animator
            _playableClipController.Init("Character Playable", characterUnit.UnitMovementSetting.Animator, characterAnimationSetting);

            //AttackController
            _characterAttackController.Init(characterUnit.Id, _movementData, characterUnit.AttackRefSetting, _playableClipController, characterUnit.WeaponTransformSetting);

            //State
            _characterStateMachine.AddState(CharacterState.Idle, new IdleState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Walk, new WalkState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Run, new RunState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Jump, new JumpState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Fall, new FallState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Land, new LandState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Dash, new DashState(_characterStateContext));
            _characterStateMachine.AddState(CharacterState.Attack, new AttackState(_characterStateContext));
            _characterStateMachine.SetState(CharacterState.Idle, true);

            //State Transition
            //Idle
            _characterStateMachine.AddTransition(CharacterState.Idle, CharacterState.Walk,
                () => { return _movementData.IsGround && _movementData.HaveMoveOperate(); });
            _characterStateMachine.AddTransition(CharacterState.Idle, CharacterState.Fall,
                () => { return !_movementData.IsGround; });
            _characterStateMachine.AddTransition(CharacterState.Idle, CharacterState.Jump,
                () => { return _movementData.JumpData.JumpTrigger; });

            //Walk
            _characterStateMachine.AddTransition(CharacterState.Walk, CharacterState.Idle,
                () => { return _movementData.IsGround && !_movementData.HaveMoveOperate(); });
            _characterStateMachine.AddTransition(CharacterState.Walk, CharacterState.Fall,
                () => { return !_movementData.IsGround; });
            _characterStateMachine.AddTransition(CharacterState.Walk, CharacterState.Jump,
                () => { return _movementData.JumpData.JumpTrigger; });
            _characterStateMachine.AddTransition(CharacterState.Walk, CharacterState.Run,
                () => { return _movementData.RunTriggered; });

            //Run
            _characterStateMachine.AddTransition(CharacterState.Run, CharacterState.Idle,
                () => { return _movementData.IsGround && !_movementData.HaveMoveOperate(); });
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

            //Dash
            _characterStateMachine.AddTransition(CharacterState.Dash, CharacterState.Idle,
                () => { return _movementData.DashData.DashElapsedTime() >= 0.3f && _movementData.IsGround; });
            _characterStateMachine.AddTransition(CharacterState.Dash, CharacterState.Fall,
                () => { return _movementData.DashData.DashElapsedTime() >= 0.3f && !_movementData.IsGround; });
            //To Dash
            var toDashState = new CharacterState[]
            {
                CharacterState.Idle,
                CharacterState.Walk,
                CharacterState.Run,
                CharacterState.Jump,
                CharacterState.Fall,
                CharacterState.Land,
                CharacterState.Attack,
            };
            _characterStateMachine.AddTransition(toDashState, CharacterState.Dash,
                () => { return _movementData.DashData.DashTrigger; });

            //Attack
            _characterStateMachine.AddTransition(CharacterState.Attack, CharacterState.Idle,
                () => { return !_characterAttackController.IsProcessCombo && _movementData.IsGround; });
            _characterStateMachine.AddTransition(CharacterState.Attack, CharacterState.Fall,
                () => { return !_characterAttackController.IsProcessCombo && !_movementData.IsGround; });
            //To Attack
            var toAttackState = new CharacterState[]
            {
                CharacterState.Idle,
                CharacterState.Walk,
                CharacterState.Run,
                CharacterState.Jump,
                CharacterState.Fall,
                CharacterState.Dash
            };
            _characterStateMachine.AddTransition(toAttackState, CharacterState.Attack,
                () => { return _characterAttackController.HaveTrigger(); });
        }

        void IUpdateTarget.DoUpdate()
        {
            _characterStateMachine.Update();
            _playableClipController.Update();
        }

        void IUpdateTarget.DoFixedUpdate()
        {
            _characterStateMachine.FixedUpdate();
        }

        void IUpdateTarget.DoLateUpdate()
        {

        }

        void IUpdateTarget.DoOnGUI()
        {

        }

        void IUpdateTarget.DoDrawGizmos()
        {
            _movementData.DrawDebugGizmos();
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

        public void Dash()
        {
            _movementData.DashData.DashTrigger = true;
        }

        public void Teleport(Vector3 worldPosition, Vector3 direction)
        {
            _movementData.UnitMovementSetting.RootTransform.position = worldPosition;
            var rotation = Quaternion.LookRotation(direction);
            _movementData.UnitMovementSetting.RotateTransform.rotation = new Quaternion(0, rotation.y, 0, rotation.w);
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

        #endregion

        #region LockCamera

        public void TriggerLookAtUnit()
        {
            _movementData.IsLockLookAtUnit = !_movementData.IsLockLookAtUnit;
            if (_movementData.IsLockLookAtUnit)
            {
                //要移到外部
                if (!GameMainSystem.TryGetNearEnemyUnit(Unit.Position, 10f, out var id))
                {
                    _movementData.IsLockLookAtUnit = false;
                    GameMainSystem.ChangeToCameraSetting(GameThirdPersonCameraSetting.Normal);
                }
                else
                {
                    _movementData.LookAtUnitId = id;
                }
            }
        }

        #endregion
    }
}
