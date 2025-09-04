using Logging;

namespace UnitModule.Movement
{
    public class DefaultMovementObserver : IMovementObserver
    {
        public void OnStateChanged(int oriState, int newState)
        {
            Log.LogInfo($"DefaultMovementObserver.OnStateChanged State:{(MovementState)newState}");
        }
    }
}
