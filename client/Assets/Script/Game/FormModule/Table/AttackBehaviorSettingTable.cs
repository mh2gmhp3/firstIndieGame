using Newtonsoft.Json;

namespace FormModule.Game.Table
{
    public class AttackBehaviorSettingRow : ITableRow
    {
        public int Id { get; }
        public int WeaponGroup { get; }

        [JsonConstructor]
        public AttackBehaviorSettingRow(int id, int weaponGroup)
        {
            Id = id;
            WeaponGroup = weaponGroup;
        }
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
