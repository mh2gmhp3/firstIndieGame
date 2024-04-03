using System;

namespace GameSystem
{
    /// <summary>
    /// 系統屬性 於初始化方便獲得各系統
    /// </summary>
    public class GameSystemAttribute : Attribute
    {
        public int Priority;

        public GameSystemAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
