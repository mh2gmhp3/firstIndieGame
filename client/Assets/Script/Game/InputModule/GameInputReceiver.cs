using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InputModule
{
    public class GameInputReceiver : IInputReceiver
    {
        /// <summary>
        /// Axis變動事件管理者
        /// </summary>
        private class AxisValueChangedEventManager
        {
            /// <summary>
            /// Axis變動事件控制器 收到Axis變動後調用通知有註冊的事件
            /// </summary>
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

                /// <summary>
                /// 獲取Id
                /// </summary>
                /// <returns></returns>
                public int GetId()
                {
                    return _id;
                }

                /// <summary>
                /// 獲取使用的Axis列表
                /// </summary>
                /// <returns></returns>
                public List<string> GetAxisList()
                {
                    return _axisList;
                }

                /// <summary>
                /// 使用的Axis列表 是否完全相同
                /// </summary>
                /// <param name="asixList"></param>
                /// <returns></returns>
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

                /// <summary>
                /// 加入Axis變動事件
                /// </summary>
                /// <param name="axisValueChangedEvent"></param>
                public void AddEvent(AxisValueChangedEvent axisValueChangedEvent)
                {
                    if (_event.Contains(axisValueChangedEvent))
                        return;

                    _event.Add(axisValueChangedEvent);
                }

                /// <summary>
                /// 移除Axis變動事件
                /// </summary>
                /// <param name="axisValueChangedEvent"></param>
                public void RemoveEvent(AxisValueChangedEvent axisValueChangedEvent)
                {
                    _event.Remove(axisValueChangedEvent);
                }

                /// <summary>
                /// 是否無事件
                /// </summary>
                /// <returns></returns>
                public bool EventIsEmpty()
                {
                    return _event.Count == 0;
                }

                /// <summary>
                /// 調用通知Axis變動
                /// </summary>
                /// <param name="value"></param>
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

            /// <summary>
            /// Axis對應有使用到此Axis的ControllerId Dic
            /// </summary>
            private Dictionary<string, List<int>> _axisToControllerIdListDic =
                new Dictionary<string, List<int>>();
            /// <summary>
            /// Id對應Controller
            /// </summary>
            private Dictionary<int, AxisValueChangedEventController> _idToControllerDic =
                new Dictionary<int, AxisValueChangedEventController>();

            /// <summary>
            /// 增長的ControllerId
            /// </summary>
            private int _increaseControllerId = 0;

            /// <summary>
            /// 註冊Axis事件
            /// </summary>
            /// <param name="axisList"></param>
            /// <param name="axisValueChangedEvent"></param>
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

            /// <summary>
            /// 反註冊Axis事件
            /// </summary>
            /// <param name="axisList"></param>
            /// <param name="axisValueChangedEvent"></param>
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

            /// <summary>
            /// 調用事件變動的Axis數值暫存
            /// </summary>
            private List<float> _invokeEventValueCache = new List<float>();
            /// <summary>
            /// Axis變動時 通知所有有使用的Controller
            /// </summary>
            /// <param name="axisList"></param>
            /// <param name="axisToValueDic"></param>
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

                    _invokeEventValueCache.Clear();
                    var controllerAxisList = controller.GetAxisList();
                    for (int j = 0; j < controllerAxisList.Count; j++)
                    {
                        if (axisToValueDic.TryGetValue(controllerAxisList[j], out var value))
                        {
                            _invokeEventValueCache.Add(value);
                            continue;
                        }

                        _invokeEventValueCache.Add(0);
                    }

                    controller.Invoke(_invokeEventValueCache);
                }
            }

            /// <summary>
            /// 回傳ControllerId列表用的暫存
            /// </summary>
            private List<int> _controllerIdListCache = new List<int>();
            /// <summary>
            /// 回傳ControllerId列表用同內容判斷Set的暫存
            /// </summary>
            private HashSet<int> _controllerIdSetCache = new HashSet<int>();
            /// <summary>
            /// 獲取Axis列表內受引響的ControllerId列表
            /// </summary>
            /// <param name="axisList"></param>
            /// <returns></returns>
            private List<int> GetControllerIdList(List<string> axisList)
            {
                _controllerIdListCache.Clear();
                _controllerIdSetCache.Clear();

                for (int i = 0; i < axisList.Count; i++)
                {
                    string axis = axisList[i];
                    if (!_axisToControllerIdListDic.TryGetValue(axis, out var idList))
                        continue;

                    for (int j = 0; j < idList.Count; j++)
                    {
                        int id = idList[j];
                        if (_controllerIdSetCache.Contains(id))
                            continue;

                        _controllerIdListCache.Add(id);
                        _controllerIdSetCache.Add(id);
                    }
                }

                return _controllerIdListCache;
            }
        }

        /// <summary>
        /// 一般KeyCode輸入
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="command"></param>
        public delegate void KeyboardInput(KeyCode keyCode, string command);
        /// <summary>
        /// Axis變動事件
        /// </summary>
        /// <param name="axisValue"></param>
        public delegate void AxisValueChangedEvent(List<float> axisValue);

        /// <summary>
        /// 按下KeyCode
        /// </summary>
        public KeyboardInput OnKeyDown;
        /// <summary>
        /// 放開KeyCode
        /// </summary>
        public KeyboardInput OnKeyUp;
        /// <summary>
        /// 按著KeyCode
        /// </summary>
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
