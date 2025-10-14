using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        public enum GameState
        {
            None,
            UI,
            Normal,
        }

        private GameState _curGameState = GameState.Normal;

        public static void SetCurGameState(GameState gameState)
        {
            _instance.DoSetCurGameState(gameState);
        }

        private void DoSetCurGameState(GameState gameState)
        {
            _curGameState = gameState;
            switch (gameState)
            {
                case GameState.UI:
                    ChangeToUIInput();
                    break;
                case GameState.Normal:
                    ChangeToNormalInput();
                    break;
                default:
                    Log.LogWarning($"GameMainSystem DoSetCurGameState Warning, GameState {gameState} is invalid");
                    break;
            }
        }
    }
}
