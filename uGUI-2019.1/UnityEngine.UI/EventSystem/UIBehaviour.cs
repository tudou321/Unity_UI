namespace UnityEngine.EventSystems
{
    // 
    public abstract class UIBehaviour : MonoBehaviour
    {
        // MonoBehaviour的几个生命周期
        protected virtual void Awake()
        {}

        protected virtual void OnEnable()
        {}

        protected virtual void Start()
        {}

        protected virtual void OnDisable()
        {}

        protected virtual void OnDestroy()
        {}

        // GameObject和Component是否处于激活状态
        public virtual bool IsActive()
        {
            return isActiveAndEnabled;
        }

#if UNITY_EDITOR
        // 当脚本被加载(禁用或启动)或者Inspector面板的值出现变化的时候会被调用，这个回调函数只在编辑器模式下会被调用，所以使用的时候最好添加#if UNITY_EDITOR
        protected virtual void OnValidate()
        {}
        // 将脚本恢复为默认值时调用，这个函数只在编辑器模式下会被调用，所以使用的时候最好添加#if UNITY_EDITOR
        protected virtual void Reset()
        {}
#endif
        // 当RectTransform变化时候调用，Anchors、Pivot、Width、Height变化时调用，Transform、Rotation、Scale变化时不调用
        protected virtual void OnRectTransformDimensionsChange()
        {}
        // 当父物体变化之前调用
        protected virtual void OnBeforeTransformParentChanged()
        {}
        // 当父物体变化之后调用
        protected virtual void OnTransformParentChanged()
        {}
        // 当Canvas状态变化时调用，比如禁用Canvas组件
        protected virtual void OnDidApplyAnimationProperties()
        {}
        // 当Canvas Group变化时调用
        protected virtual void OnCanvasGroupChanged()
        {}

        // 当应用动画属性时调用
        protected virtual void OnCanvasHierarchyChanged()
        {}

        // 是否被销毁
        public bool IsDestroyed()
        {
            return this == null;
        }
    }
}



