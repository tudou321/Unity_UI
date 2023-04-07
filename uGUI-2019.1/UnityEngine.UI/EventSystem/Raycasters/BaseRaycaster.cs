using System;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
    // 光线投射器的基础类
    // 光线投射器负责对场景元素进行光线投射，以确定光标是否在他们上面。默认光线投射器包括 PhysicsRaycaster、Physics2DRaycaster 和 GraphicRaycaster
    // 可以通过扩展这个类来添加自定义光线投射器
    public abstract class BaseRaycaster : UIBehaviour
    {
        private BaseRaycaster m_RootRaycaster;

        // 光线投射到场景中
        public abstract void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList);

        // 将为此光线投射器生成光线的摄影机
        public abstract Camera eventCamera { get; }

        [Obsolete("Please use sortOrderPriority and renderOrderPriority", false)]
        public virtual int priority
        {
            get { return 0; }
        }

        // 光线投射器基于排序顺序的优先级
        public virtual int sortOrderPriority
        {
            get { return int.MinValue; }
        }

        // 基于渲染顺序的光线投射器的优先级
        public virtual int renderOrderPriority
        {
            get { return int.MinValue; }
        }

        // 根画布上的光线投射器
        public BaseRaycaster rootRaycaster
        {
            get
            {
                if (m_RootRaycaster == null)
                {
                    var baseRaycasters = GetComponentsInParent<BaseRaycaster>();
                    if (baseRaycasters.Length != 0)
                        m_RootRaycaster = baseRaycasters[baseRaycasters.Length - 1];
                }

                return m_RootRaycaster;
            }
        }

        public override string ToString()
        {
            return "Name: " + gameObject + "\n" +
                "eventCamera: " + eventCamera + "\n" +
                "sortOrderPriority: " + sortOrderPriority + "\n" +
                "renderOrderPriority: " + renderOrderPriority;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            RaycasterManager.AddRaycaster(this);
        }

        protected override void OnDisable()
        {
            RaycasterManager.RemoveRaycasters(this);
            base.OnDisable();
        }

        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            m_RootRaycaster = null;
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            m_RootRaycaster = null;
        }
    }
}
