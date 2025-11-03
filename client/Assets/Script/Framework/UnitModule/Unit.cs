using Extension;
using UnitModule.Movement;

namespace UnitModule
{
    public abstract class Unit
    {
        private int _id;
        public abstract int UnitType { get; }

        public int Id => _id;

        public void Init(int id)
        {
            _id = id;
            DoInit();
        }

        public void Reset()
        {
            DoReset();
        }

        protected abstract void DoInit();
        protected abstract void DoReset();
    }
}
