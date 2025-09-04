using System.Collections.Generic;
using UnitModule.Movement;

namespace Utility
{
    public class ObserverController<T>
    {
        private List<T> _observerList = new List<T>();

        public List<T> ObserverList
        {
            get { return _observerList; }
        }

        public void AddObserver(T observer)
        {
            if (observer == null)
            {
                return;
            }

            if (_observerList.Contains(observer))
            {
                return;
            }

            _observerList.Add(observer);
        }

        public void RemoveObserver(T observer)
        {
            if (observer == null)
            {
                return;
            }

            _observerList.Remove(observer);
        }
    }
}
