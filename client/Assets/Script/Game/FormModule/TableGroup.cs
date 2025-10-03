namespace FormModule.Game.Table
{
    public class TableGroup
    {
        //Table定義
        public AttackBehaviorSettingTable AttackBehaviorSettingTable { get; private set; }

        public TableGroup()
        {
            //使用FormSystem.GetTable<T>獲取Table
            AttackBehaviorSettingTable = FormSystem.GetTable<AttackBehaviorSettingTable>();
        }
    }
}
