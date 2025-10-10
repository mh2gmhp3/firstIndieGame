using Newtonsoft.Json;

namespace FormModule.Game.Table
{
    public class ItemRow : ITableRow
    {
        public int Id { get; }
        public int Type { get; }
        public int Stack { get; }

        [JsonConstructor]
        public ItemRow(int id, int type, int stack)
        {
            Id = id;
            Type = type;
            Stack = stack;
        }
    }

    [Table("Item")]
    public class ItemTable : TableBase<ItemRow>
    {
        protected override void DoInit()
        {
            base.DoInit();
        }
    }
}
