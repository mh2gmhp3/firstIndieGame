using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule
{
    public class NpcUnit : Unit
    {
        protected override void DoSetup()
        {
            UnitSetting.Rigidbody.isKinematic = true;
        }

        protected override void DoReset()
        {

        }

        protected override void DoClear()
        {

        }
    }
}
