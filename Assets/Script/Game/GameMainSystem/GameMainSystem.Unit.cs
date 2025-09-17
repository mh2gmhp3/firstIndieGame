using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private UnitManager _unitManager = new UnitManager();

        public UnitManager UnitManager => _unitManager;
    }
}
