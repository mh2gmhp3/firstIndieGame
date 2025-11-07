using Newtonsoft.Json;
using System.Collections.Generic;

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

        }

        public void GetTypeRowList(int type, List<AttackBehaviorSettingRow> result)
        {
            if (result == null)
                return;
            result.Clear();

            var rowList = GetDataList();
            for (int i = 0; i < rowList.Count; i++)
            {
                var row = rowList[i];
                if (row.WeaponType == type)
                {
                    result.Add(row);
                }
            }
        }
    }
}
