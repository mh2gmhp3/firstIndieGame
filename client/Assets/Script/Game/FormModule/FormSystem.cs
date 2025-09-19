using FormModule.Game.Table;

namespace FormModule
{
    public partial class FormSystem
    {
        private TableGroup _table;
        public static TableGroup Table => _instance._table;

        public static void InitGameTableGroup()
        {
            _instance._table = new TableGroup();
        }
    }
}
