using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    // Button that's meant to work with mouse or touch-based devices.
    [AddComponentMenu("UI/Button", 30)]
    public class Button : Selectable, IPointerClickHandler, ISubmitHandler
    {
        [Serializable]
        // 定义一个点击事件
        public class ButtonClickedEvent : UnityEvent {}

        // 实例化一个 ButtonClickedEvent 的事件
        [FormerlySerializedAs("onClick")]
        [SerializeField]
        private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

        protected Button()
        {}

        // 常用的 onClick.AddListener() 就是监听这个事件
        public ButtonClickedEvent onClick
        {
            get { return m_OnClick; }
            set { m_OnClick = value; }
        }

        // 如果按钮处于活跃状态并且可交互（Interactable 设置为 true），则触发事件
        private void Press()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            m_OnClick.Invoke();
        }

        // 鼠标点击时调用该函数，继承自 IPointerClickHandler 接口
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            Press();
        }

        // 按下 "提交" 键后触发（需要先选中该游戏物体)，继承自 ISubmitHandler
        // "提交" 键可以在 Edit -> Project Setting -> Input -> Submit 中定义
        public virtual void OnSubmit(BaseEventData eventData)
        {
            Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        private IEnumerator OnFinishSubmit()
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }
    }
}
