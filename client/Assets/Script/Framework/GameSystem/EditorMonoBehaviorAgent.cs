using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.RuntimeEditor
{
    [ExecuteInEditMode]
    public class EditorMonoBehaviorAgent : MonoBehaviour
    {
#if UNITY_EDITOR
        private List<IUpdateTarget> _updateTargetList = new List<IUpdateTarget>();

        public void Register(IUpdateTarget target)
        {
            if (_updateTargetList.Contains(target))
                return;

            _updateTargetList.Add(target);
        }

        public void Unregister(IUpdateTarget target)
        {
            _updateTargetList.Remove(target);
        }

        void Update()
        {
            for (int i = 0; i < _updateTargetList.Count; i++)
            {
                _updateTargetList[i].DoUpdate();
            }
        }
#endif
    }
}
