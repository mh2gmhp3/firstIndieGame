using UnityEngine;

namespace UnitModule
{
    public class NpcUnit : Unit
    {
        public override int UnitType => (int)UnitDefine.UnitType.Npc;

        public override Vector3 Position { get ; }

        protected override void DoInit()
        {

        }

        protected override void DoReset()
        {

        }
    }
}
