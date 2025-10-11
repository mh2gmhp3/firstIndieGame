using Newtonsoft.Json;

namespace FormModule.Game.Table
{
    public class WeaponRow : ITableRow
    {
        public int Id { get; }
        public int Type { get; }
        public int BaseBehaviorCount { get; }

        [JsonConstructor]
        public WeaponRow(int id, int type, int baseBehaviorCount)
        {
            Id = id;
            Type = type;
            BaseBehaviorCount = baseBehaviorCount;
        }
    }

    [Table("Weapon")]
    public class WeaponTable : TableBase<WeaponRow>
    {
        protected override void DoInit()
        {
            base.DoInit();
        }
    }
}
