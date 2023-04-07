namespace UnityEngine.EventSystems
{
    // 使用的输入系统接口，这样，就可以使用自己的输入系统绕过输入系统，但仍然使用相同的输入模块。例如，这可以用于将虚假输入输入到UI或与不同输入系统的接口中
    public class BaseInput : UIBehaviour
    {
        // 用户键入的当前 IME 组合字符串，所谓 IME 也就是输入法，Unity提供内置的输入法，也提供自定义输入法
        public virtual string compositionString
        {
            get { return Input.compositionString; }
        }

        // 输入法模式：Auto（仅在选中文本字段时启用IME输入（默认），On（启用），Off（禁用））
        public virtual IMECompositionMode imeCompositionMode
        {
            get { return Input.imeCompositionMode; }
            set { Input.imeCompositionMode = value; }
        }

        // 输入法输入光标位置
        public virtual Vector2 compositionCursorPos
        {
            get { return Input.compositionCursorPos; }
            set { Input.compositionCursorPos = value; }
        }

        // 是否检测到鼠标
        public virtual bool mousePresent
        {
            get { return Input.mousePresent; }
        }

        // 鼠标是否按下
        public virtual bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        // 鼠标是否弹起
        public virtual bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(button);
        }

        // 鼠标长按
        public virtual bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(button);
        }

        // 鼠标位置（屏幕坐标）
        public virtual Vector2 mousePosition
        {
            get { return Input.mousePosition; }
        }

        // 鼠标滚动量
        public virtual Vector2 mouseScrollDelta
        {
            get { return Input.mouseScrollDelta; }
        }

        // 当前设备是否支持触摸
        public virtual bool touchSupported
        {
            get { return Input.touchSupported; }
        }

        // 触摸次数
        public virtual int touchCount
        {
            get { return Input.touchCount; }
        }

        // 根据索引获取触摸
        public virtual Touch GetTouch(int index)
        {
            return Input.GetTouch(index);
        }

        // 获取原始的轴数据（未平滑）
        public virtual float GetAxisRaw(string axisName)
        {
            return Input.GetAxisRaw(axisName);
        }

        // 检测键盘的某个键点按下
        public virtual bool GetButtonDown(string buttonName)
        {
            return Input.GetButtonDown(buttonName);
        }
    }
}



