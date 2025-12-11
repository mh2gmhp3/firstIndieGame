using GameMainModule.Attack;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private AttackCastManager _attackCastManager;

        private void InitAttackCastManager()
        {
            _attackCastManager = new AttackCastManager(_attackTriggerAssetSetting);
            RegisterUpdateTarget(_attackCastManager);
        }
    }
}
