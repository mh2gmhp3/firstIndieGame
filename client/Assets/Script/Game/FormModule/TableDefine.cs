namespace FormModule
{
    public static class TableDefine
    {
        /// <summary>
        /// 道具類型
        /// </summary>
        public enum ItemType
        {
            /// <summary>
            /// 無效道具
            /// </summary>
            None   = 0,
            /// <summary>
            /// 一般道具
            /// </summary>
            Normal = 1,
            /// <summary>
            /// 武器
            /// </summary>
            Weapon = 2,
            /// <summary>
            /// 攻擊動作
            /// </summary>
            AttackBehavior = 3,
        }
    }
}
