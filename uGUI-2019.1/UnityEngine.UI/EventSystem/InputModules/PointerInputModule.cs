using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace UnityEngine.EventSystems
{
    // 点输入的基础输入模块
    public abstract class PointerInputModule : BaseInputModule
    {
        // 鼠标左键id
        public const int kMouseLeftId = -1;

        // 鼠标右键id
        public const int kMouseRightId = -2;

        // 鼠标中间键id
        public const int kMouseMiddleId = -3;

        // 在非触摸设备上模拟触摸时的触摸id
        public const int kFakeTouchesId = -4;

        protected Dictionary<int, PointerEventData> m_PointerData = new Dictionary<int, PointerEventData>();

        // 在缓存中搜索当前活动的指针，如果找到，则返回true
        protected bool GetPointerData(int id, out PointerEventData data, bool create)
        {
            if (!m_PointerData.TryGetValue(id, out data) && create)
            {
                data = new PointerEventData(eventSystem)
                {
                    pointerId = id,
                };
                m_PointerData.Add(id, data);
                return true;
            }
            return false;
        }

        // 从缓存中删除 PointerEventData
        protected void RemovePointerData(PointerEventData data)
        {
            m_PointerData.Remove(data.pointerId);
        }

        // 只要触摸一下，就会填充 PointerEventData，如果我们按下或者释放，就会返回
        protected PointerEventData GetTouchPointerEventData(Touch input, out bool pressed, out bool released)
        {
            PointerEventData pointerData;
            var created = GetPointerData(input.fingerId, out pointerData, true);

            pointerData.Reset();

            pressed = created || (input.phase == TouchPhase.Began);
            released = (input.phase == TouchPhase.Canceled) || (input.phase == TouchPhase.Ended);

            if (created)
                pointerData.position = input.position;

            if (pressed)
                pointerData.delta = Vector2.zero;
            else
                pointerData.delta = input.position - pointerData.position;

            pointerData.position = input.position;

            pointerData.button = PointerEventData.InputButton.Left;

            if (input.phase == TouchPhase.Canceled)
            {
                pointerData.pointerCurrentRaycast = new RaycastResult();
            }
            else
            {
                eventSystem.RaycastAll(pointerData, m_RaycastResultCache);

                var raycast = FindFirstRaycast(m_RaycastResultCache);
                pointerData.pointerCurrentRaycast = raycast;
                m_RaycastResultCache.Clear();
            }
            return pointerData;
        }

        // 复制一个 PointerEventData
        protected void CopyFromTo(PointerEventData @from, PointerEventData @to)
        {
            @to.position = @from.position;
            @to.delta = @from.delta;
            @to.scrollDelta = @from.scrollDelta;
            @to.pointerCurrentRaycast = @from.pointerCurrentRaycast;
            @to.pointerEnter = @from.pointerEnter;
        }

        // 给定鼠标按钮，返回帧的当前状态
        protected PointerEventData.FramePressState StateForMouseButton(int buttonId)
        {
            var pressed = input.GetMouseButtonDown(buttonId);
            var released = input.GetMouseButtonUp(buttonId);
            if (pressed && released)
                return PointerEventData.FramePressState.PressedAndReleased;
            if (pressed)
                return PointerEventData.FramePressState.Pressed;
            if (released)
                return PointerEventData.FramePressState.Released;
            return PointerEventData.FramePressState.NotChanged;
        }

        protected class ButtonState
        {
            private PointerEventData.InputButton m_Button = PointerEventData.InputButton.Left;

            public MouseButtonEventData eventData
            {
                get { return m_EventData; }
                set { m_EventData = value; }
            }

            public PointerEventData.InputButton button
            {
                get { return m_Button; }
                set { m_Button = value; }
            }

            private MouseButtonEventData m_EventData;
        }

        protected class MouseState
        {
            private List<ButtonState> m_TrackedButtons = new List<ButtonState>();

            public bool AnyPressesThisFrame()
            {
                for (int i = 0; i < m_TrackedButtons.Count; i++)
                {
                    if (m_TrackedButtons[i].eventData.PressedThisFrame())
                        return true;
                }
                return false;
            }

            public bool AnyReleasesThisFrame()
            {
                for (int i = 0; i < m_TrackedButtons.Count; i++)
                {
                    if (m_TrackedButtons[i].eventData.ReleasedThisFrame())
                        return true;
                }
                return false;
            }

            public ButtonState GetButtonState(PointerEventData.InputButton button)
            {
                ButtonState tracked = null;
                for (int i = 0; i < m_TrackedButtons.Count; i++)
                {
                    if (m_TrackedButtons[i].button == button)
                    {
                        tracked = m_TrackedButtons[i];
                        break;
                    }
                }

                if (tracked == null)
                {
                    tracked = new ButtonState { button = button, eventData = new MouseButtonEventData() };
                    m_TrackedButtons.Add(tracked);
                }
                return tracked;
            }

            public void SetButtonState(PointerEventData.InputButton button, PointerEventData.FramePressState stateForMouseButton, PointerEventData data)
            {
                var toModify = GetButtonState(button);
                toModify.eventData.buttonState = stateForMouseButton;
                toModify.eventData.buttonData = data;
            }
        }

        // 鼠标按钮事件的信息
        public class MouseButtonEventData
        {
            // 这一帧按钮的状态
            public PointerEventData.FramePressState buttonState;

            // 与鼠标事件关联的指针数据
            public PointerEventData buttonData;

            // 按钮是否在这一帧按下的
            public bool PressedThisFrame()
            {
                return buttonState == PointerEventData.FramePressState.Pressed || buttonState == PointerEventData.FramePressState.PressedAndReleased;
            }

            // 这一帧按钮松开没
            public bool ReleasedThisFrame()
            {
                return buttonState == PointerEventData.FramePressState.Released || buttonState == PointerEventData.FramePressState.PressedAndReleased;
            }
        }

        private readonly MouseState m_MouseState = new MouseState();

        // 返回当前鼠标状态，使用默认指针
        protected virtual MouseState GetMousePointerEventData()
        {
            return GetMousePointerEventData(0);
        }

        // 返回当前鼠标状态
        protected virtual MouseState GetMousePointerEventData(int id)
        {
            // 填充左侧按钮
            PointerEventData leftData;
            var created = GetPointerData(kMouseLeftId, out leftData, true);

            leftData.Reset();

            if (created)
                leftData.position = input.mousePosition;

            Vector2 pos = input.mousePosition;
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                // 当鼠标被锁定时，我们不想进行任何基于光标的交互
                leftData.position = new Vector2(-1.0f, -1.0f);
                leftData.delta = Vector2.zero;
            }
            else
            {
                leftData.delta = pos - leftData.position;
                leftData.position = pos;
            }
            leftData.scrollDelta = input.mouseScrollDelta;
            leftData.button = PointerEventData.InputButton.Left;
            eventSystem.RaycastAll(leftData, m_RaycastResultCache);
            var raycast = FindFirstRaycast(m_RaycastResultCache);
            leftData.pointerCurrentRaycast = raycast;
            m_RaycastResultCache.Clear();

            // 将适当的数据复制到右侧和中间的插槽中
            PointerEventData rightData;
            GetPointerData(kMouseRightId, out rightData, true);
            CopyFromTo(leftData, rightData);
            rightData.button = PointerEventData.InputButton.Right;

            PointerEventData middleData;
            GetPointerData(kMouseMiddleId, out middleData, true);
            CopyFromTo(leftData, middleData);
            middleData.button = PointerEventData.InputButton.Middle;

            m_MouseState.SetButtonState(PointerEventData.InputButton.Left, StateForMouseButton(0), leftData);
            m_MouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
            m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);

            return m_MouseState;
        }

        // 返回给定触摸/鼠标id的最后一个PointerEventData
        protected PointerEventData GetLastPointerEventData(int id)
        {
            PointerEventData data;
            GetPointerData(id, out data, false);
            return data;
        }

        private static bool ShouldStartDrag(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold)
        {
            if (!useDragThreshold)
                return true;

            return (pressPos - currentPos).sqrMagnitude >= threshold * threshold;
        }

        // 处理具有给定指针事件的当前帧的移动
        protected virtual void ProcessMove(PointerEventData pointerEvent)
        {
            var targetGO = (Cursor.lockState == CursorLockMode.Locked ? null : pointerEvent.pointerCurrentRaycast.gameObject);
            HandlePointerExitAndEnter(pointerEvent, targetGO);
        }

        // 使用给定的指针事件处理当前帧的拖动
        protected virtual void ProcessDrag(PointerEventData pointerEvent)
        {
            if (!pointerEvent.IsPointerMoving() ||
                Cursor.lockState == CursorLockMode.Locked ||
                pointerEvent.pointerDrag == null)
                return;

            if (!pointerEvent.dragging
                && ShouldStartDrag(pointerEvent.pressPosition, pointerEvent.position, eventSystem.pixelDragThreshold, pointerEvent.useDragThreshold))
            {
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            // 拖动通知
            if (pointerEvent.dragging)
            {
                // 在进行拖动之前，我们应该取消任何向下的指针状态并清除所选内容
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }
                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        public override bool IsPointerOverGameObject(int pointerId)
        {
            var lastPointer = GetLastPointerEventData(pointerId);
            if (lastPointer != null)
                return lastPointer.pointerEnter != null;
            return false;
        }

        // 清除所有指针并取消选择 EventSystem 中的任何选定对象
        protected void ClearSelection()
        {
            var baseEventData = GetBaseEventData();

            foreach (var pointer in m_PointerData.Values)
            {
                // 清除所有选择
                HandlePointerExitAndEnter(pointer, null);
            }

            m_PointerData.Clear();
            eventSystem.SetSelectedGameObject(null, baseEventData);
        }

        public override string ToString()
        {
            var sb = new StringBuilder("<b>Pointer Input Module of type: </b>" + GetType());
            sb.AppendLine();
            foreach (var pointer in m_PointerData)
            {
                if (pointer.Value == null)
                    continue;
                sb.AppendLine("<B>Pointer:</b> " + pointer.Key);
                sb.AppendLine(pointer.Value.ToString());
            }
            return sb.ToString();
        }

        // 如果当前指向的游戏对象不同，则取消选择当前选定的游戏对象
        /// currentOverGo：指针当前所在的游戏对象
        /// pointerEvent：当前事件数据
        protected void DeselectIfSelectionChanged(GameObject currentOverGo, BaseEventData pointerEvent)
        {
            // 选择跟踪
            var selectHandlerGO = ExecuteEvents.GetEventHandler<ISelectHandler>(currentOverGo);
            // 如果我们点击了新的物体，请取消选择旧的物体
            // 将 选择操作 传递给 按下事件
            if (selectHandlerGO != eventSystem.currentSelectedGameObject)
                eventSystem.SetSelectedGameObject(null, pointerEvent);
        }
    }
}
