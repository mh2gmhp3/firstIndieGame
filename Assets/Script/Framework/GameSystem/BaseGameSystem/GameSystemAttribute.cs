using System;

namespace Framework.GameSystem
{
    /// <summary>
    /// �t���ݩ� ���l�Ƥ�K��o�U�t��
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
