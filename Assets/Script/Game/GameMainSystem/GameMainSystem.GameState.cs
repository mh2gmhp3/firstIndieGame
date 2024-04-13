using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainSystem
{
    public partial class GameMainSystem
    {
        public enum GameState
        {
            None,
            UI,
            Normal,
        }

        private GameState _gameState = GameState.None;
    }
}
