using Newtonsoft.Json;

namespace FormModule.Game.Table
{
    public class AttackBehaviorSettingRow : ITableRow
    {
        public int Id { get; }
        public int WeaponType { get; }

        [JsonConstructor]
        public AttackBehaviorSettingRow(int id, int weaponType)
        {
            Id = id;
            WeaponType = weaponType;
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
