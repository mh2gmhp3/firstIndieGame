namespace FormModule.Game.Table
{
    public class AttackBehaviorSettingRow : ITableRow
    {
        public int Id { get; set; }
        public int WeaponGroup;
    }

    [Table("AttackBehaviorSetting")]
    public class AttackBehaviorSettingTable : TableBase<AttackBehaviorSettingRow>
    {
        protected override void DoInit()
        {
            base.DoInit();
        }
    }
}
