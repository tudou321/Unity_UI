using System;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    [RequireComponent(typeof(EventSystem))]

    public abstract class BaseInputModule : UIBehaviour
    {
        [NonSerialized]
        // 射线投射结果缓存列表，避免频繁申请
        protected List<RaycastResult> m_RaycastResultCache = new List<RaycastResult>();

        // 轴数据，根据传入的封装事件数据
        private AxisEventData m_AxisEventData;

        // 引用EventSystem
        private EventSystem m_EventSystem;
        // 基础的事件数据，用于在一些地方进行初始化变量
        private BaseEventData m_BaseEventData;

        // 提供的BaseInput重载，自定义的Input
        protected BaseInput m_InputOverride;
        // 对象身上找到的第一个BaseInput
        private BaseInput m_DefaultInput;

        // BaseInput属性
        public BaseInput input
        {
            get
            {
                // 提供重载机制
                if (m_InputOverride != null)
                    return m_InputOverride;

                // 只使用BaseInput，不使用子类
                if (m_DefaultInput == null)
                {
                    var inputs = GetComponents<BaseInput>();
                    foreach (var baseInput in inputs)
                    {
                        if (baseInput != null && baseInput.GetType() == typeof(BaseInput))
                        {
                            m_DefaultInput = baseInput;
                            break;
                        }
                    }

                    if (m_DefaultInput == null)
                        m_DefaultInput = gameObject.AddComponent<BaseInput>();
                }

                return m_DefaultInput;
            }
        }

        // 重载的BaseInput
        public BaseInput inputOverride
        {
            get { return m_InputOverride; }
            set { m_InputOverride = value; }
        }

        // 引用的eventSystem属性
        protected EventSystem eventSystem
        {
            get { return m_EventSystem; }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_EventSystem = GetComponent<EventSystem>();
            m_EventSystem.UpdateModules();
        }

        protected override void OnDisable()
        {
            m_EventSystem.UpdateModules();
            base.OnDisable();
        }

        // 抽象的事件处理方法，需要具体子类实现
        public abstract void Process();

        // 查找第一个投射结果，需要游戏对象存在，如果没有则返回一个新建的投射结果
        protected static RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
        {
            for (var i = 0; i < candidates.Count; ++i)
            {
                if (candidates[i].gameObject == null)
                    continue;

                return candidates[i];
            }
            return new RaycastResult();
        }

        // 根据坐标确定移动方向
        protected static MoveDirection DetermineMoveDirection(float x, float y)
        {
            return DetermineMoveDirection(x, y, 0.6f);
        }

        // 根据坐标确定移动方向（上下左右，未移动）
        // moveDeadZone 用于最小位移判断
        protected static MoveDirection DetermineMoveDirection(float x, float y, float deadZone)
        {
            // 忽略数值很小的移动
            // magnitude 是指向量的长度
            // sqrMagnitude 是指向量长度的平方
            // 在Unity当中使用平方的计算要比计算开放的速度快很多
            // 如果只是单纯比较向量之间的大小的话，建议使用 Vector3.sqrMagnitude 进行比较即可，提高效率和节约性能
            if (new Vector2(x, y).sqrMagnitude < deadZone * deadZone)
                return MoveDirection.None;

            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                // 水平方向（左右）
                if (x > 0)
                    return MoveDirection.Right;
                return MoveDirection.Left;
            }
            else
            {
                // 竖直方向（上下）
                if (y > 0)
                    return MoveDirection.Up;
                return MoveDirection.Down;
            }
        }

        // 找出两个游戏对象共同的父节点（通过两重循环）
        protected static GameObject FindCommonRoot(GameObject g1, GameObject g2)
        {
            if (g1 == null || g2 == null)
                return null;

            var t1 = g1.transform;
            while (t1 != null)
            {
                var t2 = g2.transform;
                while (t2 != null)
                {
                    if (t1 == t2)
                        return t1.gameObject;
                    t2 = t2.parent;
                }
                t1 = t1.parent;
            }
            return null;
        }

        // 向上一个[进入对象]（也就是当前指针事件数据中存储的[进入对象]）和其所有父节点发送离开事件
        // 向当前的新的[进入对象]发送进入事件和其所有父节点发送进入事件
        // 如果两个对象存在共同的父节点
        protected void HandlePointerExitAndEnter(PointerEventData currentPointerData, GameObject newEnterTarget)
        {
            // 如果没有新的[进入对象]（指针游离在所有的对象之外），或者当前的指针事件的[进入对象]被删除
            // 那么就简单的向所有悬停对象（就是当前进入对象和所有父节点链条上所有节点对象）发送离开事件
            if (newEnterTarget == null || currentPointerData.pointerEnter == null)
            {
                for (var i = 0; i < currentPointerData.hovered.Count; ++i)
                    ExecuteEvents.Execute(currentPointerData.hovered[i], currentPointerData, ExecuteEvents.pointerExitHandler);

                currentPointerData.hovered.Clear();
                // 如果没有新的[进入对象]，将当前的事件中的[进入对象]也清空
                // 因为没有当前对象，所以不需要发送进入事件，直接返回
                if (newEnterTarget == null)
                {
                    currentPointerData.pointerEnter = null;
                    return;
                }
            }

            // 如果[进入对象]存在且未变化，则啥也不做
            if (currentPointerData.pointerEnter == newEnterTarget && newEnterTarget)
                return;

            /// 下面的区域处理的是有一定新的[进入对象]，但是不一定有上一个[进入对象]

            // 查找上一个[进入对象]和新的[进入对象]共同的父节点
            GameObject commonRoot = FindCommonRoot(currentPointerData.pointerEnter, newEnterTarget);

            // 如果有上一个[进入对象]，向上一个[进入对象]一直向上直到共同父节点（或者没有）链条上所有的节点发送离开事件，且不包含共同的父节点
            if (currentPointerData.pointerEnter != null)
            {
                // send exit handler call to all elements in the chain
                // until we reach the new target, or null!
                Transform t = currentPointerData.pointerEnter.transform;

                while (t != null)
                {
                    // 遇到共同父节点停止
                    if (commonRoot != null && commonRoot.transform == t)
                        break;

                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerExitHandler);
                    currentPointerData.hovered.Remove(t.gameObject);
                    t = t.parent;
                }
            }

            // 向新的[进入对象]一直到共同父节点（或者没有）链条上所有节点发送进入事件
            currentPointerData.pointerEnter = newEnterTarget;
            if (newEnterTarget != null)
            {
                Transform t = newEnterTarget.transform;
                // 遇到父节点停止
                while (t != null && t.gameObject != commonRoot)
                {
                    ExecuteEvents.Execute(t.gameObject, currentPointerData, ExecuteEvents.pointerEnterHandler);
                    currentPointerData.hovered.Add(t.gameObject);
                    t = t.parent;
                }
            }
        }

        // 根据传入的数据，封装轴事件数据
        protected virtual AxisEventData GetAxisEventData(float x, float y, float moveDeadZone)
        {
            if (m_AxisEventData == null)
                m_AxisEventData = new AxisEventData(eventSystem);

            m_AxisEventData.Reset();
            m_AxisEventData.moveVector = new Vector2(x, y);
            m_AxisEventData.moveDir = DetermineMoveDirection(x, y, moveDeadZone);
            return m_AxisEventData;
        }

        // 获取基础的事件数据，用于在一些地方进行初始化变量
        protected virtual BaseEventData GetBaseEventData()
        {
            if (m_BaseEventData == null)
                m_BaseEventData = new BaseEventData(eventSystem);

            m_BaseEventData.Reset();
            return m_BaseEventData;
        }

        // 给定id的指针，是否位于某个对象区域
        public virtual bool IsPointerOverGameObject(int pointerId)
        {
            return false;
        }

        // 本输入模块是否应该激活
        public virtual bool ShouldActivateModule()
        {
            return enabled && gameObject.activeInHierarchy;
        }

        // 停止模块时调用的函数
        public virtual void DeactivateModule()
        {}

        // 激活模块时调用的函数
        public virtual void ActivateModule()
        {}

        // 更新模块时调用的函数
        public virtual void UpdateModule()
        {}

        // 模块是否支持
        public virtual bool IsModuleSupported()
        {
            return true;
        }
    }
}
