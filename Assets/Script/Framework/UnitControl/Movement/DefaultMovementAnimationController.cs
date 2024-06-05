using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement
{
    public class DefaultMovementAnimationController : IMovementAnimationController
    {
        public void MoveInput(Vector3 axis, Quaternion quaternion)
        {
            if (axis.sqrMagnitude == 0)
                return;
            Log.LogInfo($"animation input axis:{axis} quaternion:{quaternion}");
        }

        public void OnJumping(int jumpCount, float jumpElapsedTime, float jumpLimitTime)
        {
            Log.LogInfo($"animation onJump jumpCount:{jumpCount} jumpElapsedTime:{jumpElapsedTime} jumpLimitTime:{jumpLimitTime}");
        }

        public void OnFalling(float fallingElapsedTime, float fallingMaxTime)
        {
            Log.LogInfo($"animation onFalling fallingElapsedTime:{fallingElapsedTime} fallingMaxTime:{fallingMaxTime}");
        }

        public void OnLand()
        {
            Log.LogInfo("animation onLand");
        }
    }
}
