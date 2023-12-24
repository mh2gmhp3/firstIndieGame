using System;

namespace Framework.GameSystem
{
    /// <summary>
    /// 系統屬性 於初始化方便獲得各系統
    /// </summary>
    public class GameSystemAttribute : Attribute
    {
        public string Name;
        public int Priority;

        public GameSystemAttribute(string name, int priority)
        {
            Name = name;
            Priority = priority;
        }
    }
}
