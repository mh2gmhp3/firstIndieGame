using CollisionModule;

namespace GameMainModule.Attack
{
    /// <summary>
    /// 假的角色攻擊觸發資料
    /// </summary>
    public class FakeCharacterTriggerInfo : ICollisionAreaTriggerInfo
    {
        public bool ToCharacter = false;
        public int Attack = 20;
    }
}