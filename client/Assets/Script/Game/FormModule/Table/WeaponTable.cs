using Newtonsoft.Json;

namespace FormModule.Game.Table
{
    public class WeaponRow : ITableRow
    {
        public int Id { get; }
        public int Type { get; }
        public int BaseBehaviorCount { get; }
        public string ModelName { get; }
        public int UseHand { get; }
        public int StorageIndex { get; }

        [JsonConstructor]
        public WeaponRow(int id, int type, int baseBehaviorCount, string modelName, int useHand, int storageIndex)
        {
            Id = id;
            Type = type;
            BaseBehaviorCount = baseBehaviorCount;
            ModelName = modelName;
            UseHand = useHand;
            StorageIndex = storageIndex;
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
