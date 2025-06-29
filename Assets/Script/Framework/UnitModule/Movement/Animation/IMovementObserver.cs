using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule.Movement
{
    public interface IMovementObserver
    {
        public void MoveInput(Vector3 axis, Quaternion quaternion);

        public void OnJumping(int jumpCount, float jumpElapsedTime, float jumpLimitTime);
        public void OnFalling(float fallingElapsedTime, float fallingMaxTime);
        public void OnLand();
    }
}
