using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InputModule
{
    public class GameInputReceiver : IInputReceiver
    {
        private class AxisValueChangedEventManager
        {
            private class AxisValueChangedEventController
            {
                private int _id = 0;
                private List<string> _axisList = new List<string>();
                private List<AxisValueChangedEvent> _event =
                    new List<AxisValueChangedEvent>();

                public AxisValueChangedEventController(int id, List<string> axisList)
                {
                    _id = id;
                    _axisList.AddRange(axisList);
                }

                public int GetId()
                {
                    return _id;
                }

                public List<string> GetAxisList()
                {
                    return _axisList;
                }

                public bool AxisIsEqual(List<string> asixList)
                {
                    if (_axisList.Count != asixList.Count)
                        return false;

                    for (int i = 0; i < asixList.Count; i++)
                    {
                        if (asixList[i] != _axisList[i])
                            return false;
                    }

                    return true;
                }

                public void AddEvent(AxisValueChangedEvent axisValueChangedEvent)
                {
                    if (_event.Contains(axisValueChangedEvent))
                        return;

                    _event.Add(axisValueChangedEvent);
                }

                public void RemoveEvent(AxisValueChangedEvent axisValueChangedEvent)
                {
                    _event.Remove(axisValueChangedEvent);
                }

                public bool EventIsEmpty()
                {
                    return _event.Count == 0;
                }

                public void Invoke(List<float> value)
                {
                    for (int i = 0; i < _event.Count; i++)
                    {
                        if (_event[i] == null)
                            continue;

                        _event[i].Invoke(value);
                    }
                }
            }

            private Dictionary<string, List<int>> _axisToControllerIdListDic =
                new Dictionary<string, List<int>>();
            private Dictionary<int, AxisValueChangedEventController> _idToControllerDic =
                new Dictionary<int, AxisValueChangedEventController>();

            private int _increaseControllerId = 0;

            private List<int> _controllerIdList = new List<int>();
            private HashSet<int> _controllerIdSet = new HashSet<int>();

            private List<float> _invokeEventValue = new List<float>();

            public void RegisterAxisValueChangedEvent(
                List<string> axisList,
                AxisValueChangedEvent axisValueChangedEvent)
            {
                if (axisList == null ||
                    axisValueChangedEvent == null)
                    return;
                int axisListcount = axisList.Count;
                if (axisListcount == 0)
                    return;

                var controllerIdList = GetControllerIdList(axisList);
                for (int i = 0; i < controllerIdList.Count; i++)
                {
                    if (!_idToControllerDic.TryGetValue(controllerIdList[i], out var controller))
                        continue;

                    //找到註冊一樣的Axis直接加入事件
                    if (controller.AxisIsEqual(axisList))
                    {
                        controller.AddEvent(axisValueChangedEvent);
                        return;
                    }
                }

                _increaseControllerId++;
                var newController = new AxisValueChangedEventController(
                    _increaseControllerId,
                    axisList);
                newController.AddEvent(axisValueChangedEvent);
                for (int i = 0; i < axisListcount; i++)
                {
                    string axis = axisList[i];
                    if (!_axisToControllerIdListDic.TryGetValue(axis, out var idList))
                    {
                        idList = new List<int>();
                        _axisToControllerIdListDic.Add(axis, idList);
                    }

                    idList.Add(_increaseControllerId);
                }
                //不應該會有同Id的
                _idToControllerDic.TryAdd(_increaseControllerId, newController);
            }

            public void UnRegisterAxisValueChangedEvent(
                List<string> axisList,
                AxisValueChangedEvent axisValueChangedEvent)
            {
                if (axisList == null ||
                    axisValueChangedEvent == null)
                    return;
                int axisListcount = axisList.Count;
                if (axisListcount == 0)
                    return;

                var controllerIdList = GetControllerIdList(axisList);
                AxisValueChangedEventController oriController = null;
                for (int i = 0; i < controllerIdList.Count; i++)
                {
                    if (!_idToControllerDic.TryGetValue(controllerIdList[i], out var controller))
                        continue;

                    //找到原本註冊的Controller
                    if (controller.AxisIsEqual(axisList))
                    {
                        oriController = controller;
                        break;
                    }
                }

                if (oriController == null)
                    return;

                oriController.RemoveEvent(axisValueChangedEvent);
                //事件還沒有空代表來有人註冊 不需清掉
                if (!oriController.EventIsEmpty())
                    return;

                int controllerId = oriController.GetId();
                for (int i = 0; i < axisListcount; i++)
                {
                    string axis = axisList[i];
                    if (!_axisToControllerIdListDic.TryGetValue(axis, out var idList))
                        continue;

                    idList.Remove(controllerId);
                }
                _idToControllerDic.Remove(controllerId);
            }

            public void OnAxisValueChanged(
                List<string> axisList,
                Dictionary<string, float> axisToValueDic)
            {
                //調用註冊觀察Axis有變動的所有Controller
                var invokeControllerIdList = GetControllerIdList(axisList);
                int count = invokeControllerIdList.Count;
                if (count == 0)
                    return;

                for (int i = 0; i < count; i++)
                {
                    int invokeControllerId = invokeControllerIdList[i];
                    if (!_idToControllerDic.TryGetValue(invokeControllerId, out var controller))
                        continue;

                    _invokeEventValue.Clear();
                    var controllerAxisList = controller.GetAxisList();
                    for (int j = 0; j < controllerAxisList.Count; j++)
                    {
                        if (axisToValueDic.TryGetValue(controllerAxisList[j], out var value))
                        {
                            _invokeEventValue.Add(value);
                            continue;
                        }

                        _invokeEventValue.Add(0);
                    }

                    controller.Invoke(_invokeEventValue);
                }
            }

            private List<int> GetControllerIdList(List<string> asixList)
            {
                _controllerIdList.Clear();
                _controllerIdSet.Clear();

                for (int i = 0; i < asixList.Count; i++)
                {
                    string axis = asixList[i];
                    if (!_axisToControllerIdListDic.TryGetValue(axis, out var idList))
                        continue;

                    for (int j = 0; j < idList.Count; j++)
                    {
                        int id = idList[j];
                        if (_controllerIdSet.Contains(id))
                            continue;

                        _controllerIdList.Add(id);
                        _controllerIdSet.Add(id);
                    }
                }

                return _controllerIdList;
            }
        }

        public delegate void KeyboardInput(KeyCode keyCode, string command);
        public delegate void AxisValueChangedEvent(List<float> axisValue);

        public KeyboardInput OnKeyDown;
        public KeyboardInput OnKeyUp;
        public KeyboardInput OnKeyHold;

        private AxisValueChangedEventManager _axisValueChangedEventManager =
            new AxisValueChangedEventManager();

        public GameInputReceiver(
            KeyboardInput onKeyDown,
            KeyboardInput onKeyUp,
            KeyboardInput onKeyHold)
        {
            OnKeyDown = onKeyDown;
            OnKeyUp = onKeyUp;
            OnKeyHold = onKeyHold;
        }

        /// <summary>
        /// 註冊Axis變動事件 使用AxisList 存在於AxisList內任意值變動後將傳入整個AxisList的所有Value
        /// </summary>
        /// <param name="axisList"></param>
        /// <param name="axisValueChangedEvent"></param>
        public void RegisterAxisValueChangedEvent(
            List<string> axisList,
            AxisValueChangedEvent axisValueChangedEvent)
        {
            _axisValueChangedEventManager.RegisterAxisValueChangedEvent(
                axisList,
                axisValueChangedEvent);
        }

        /// <summary>
        /// 反註冊Axis變動事件
        /// </summary>
        /// <param name="axisList"></param>
        /// <param name="axisValueChangedEvent"></param>
        public void UnRegisterAxisValueChangedEvent(
            List<string> axisList,
            AxisValueChangedEvent axisValueChangedEvent)
        {
            _axisValueChangedEventManager.UnRegisterAxisValueChangedEvent(
                axisList,
                axisValueChangedEvent);
        }

        /// <summary>
        /// 負責接收Axis變動
        /// </summary>
        /// <param name="axisList"></param>
        /// <param name="axisToValueDic"></param>
        public void OnAxisValueChanged(
            List<string> axisList,
            Dictionary<string, float> axisToValueDic)
        {
            _axisValueChangedEventManager.OnAxisValueChanged(
                axisList,
                axisToValueDic);
        }
    }
}
