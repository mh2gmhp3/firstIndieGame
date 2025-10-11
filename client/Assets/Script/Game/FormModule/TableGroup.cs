namespace FormModule.Game.Table
{
    public class TableGroup
    {
        //Table定義
        public AttackBehaviorSettingTable AttackBehaviorSettingTable { get; }
        public ItemTable ItemTable { get; }
        public WeaponTable WeaponTable { get; }

        public TableGroup()
        {
            //使用FormSystem.GetTable<T>獲取Table
            AttackBehaviorSettingTable = FormSystem.GetTable<AttackBehaviorSettingTable>();
            ItemTable = FormSystem.GetTable<ItemTable>();
            WeaponTable = FormSystem.GetTable<WeaponTable>();
        }
    }
}
