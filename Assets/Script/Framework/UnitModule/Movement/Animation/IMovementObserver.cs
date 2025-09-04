namespace UnitModule.Movement
{
    public interface IMovementObserver
    {
        void OnStateChanged(int oriState, int newState);
    }
}
