using Newtonsoft.Json;

namespace FormModule.Game.Table
{
    public class AttackBehaviorSettingRow : ITableRow
    {
        public int Id { get; }
        public int WeaponType { get; }
        public int AssetSettingId { get; }

        [JsonConstructor]
        public AttackBehaviorSettingRow(int id, int weaponType, int assetSettingId)
        {
            Id = id;
            WeaponType = weaponType;
            AssetSettingId = assetSettingId;
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
