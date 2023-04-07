using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event/Event Trigger")]
    public class EventTrigger :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IScrollHandler,
        IUpdateSelectedHandler,
        ISelectHandler,
        IDeselectHandler,
        IMoveHandler,
        ISubmitHandler,
        ICancelHandler
    {
        [Serializable]
        /// <summary>
        /// UnityEvent class for Triggers.
        /// </summary>
        public class TriggerEvent : UnityEvent<BaseEventData>
        {}

        [Serializable]
        /// <summary>
        /// An Entry in the EventSystem delegates list.
        /// </summary>
        /// <remarks>
        /// It stores the callback and which event type should this callback be fired.
        /// </remarks>
        public class Entry
        {
            /// <summary>
            /// What type of event is the associated callback listening for.
            /// </summary>
            public EventTriggerType eventID = EventTriggerType.PointerClick;

            /// <summary>
            /// The desired TriggerEvent to be Invoked.
            /// </summary>
            public TriggerEvent callback = new TriggerEvent();
        }

        [FormerlySerializedAs("delegates")]
        [SerializeField]
        private List<Entry> m_Delegates;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Please use triggers instead (UnityUpgradable) -> triggers", true)]
        public List<Entry> delegates { get { return triggers; } set { triggers = value; } }

        protected EventTrigger()
        {}

        /// <summary>
        /// All the functions registered in this EventTrigger
        /// </summary>
        public List<Entry> triggers
        {
            get
            {
                if (m_Delegates == null)
                    m_Delegates = new List<Entry>();
                return m_Delegates;
            }
            set { m_Delegates = value; }
        }

        private void Execute(EventTriggerType id, BaseEventData eventData)
        {
            for (int i = 0, imax = triggers.Count; i < imax; ++i)
            {
                var ent = triggers[i];
                if (ent.eventID == id && ent.callback != null)
                    ent.callback.Invoke(eventData);
            }
        }

        /// <summary>
        /// Called by the EventSystem when the pointer enters the object associated with this EventTrigger.
        /// </summary>
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerEnter, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when the pointer exits the object associated with this EventTrigger.
        /// </summary>
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerExit, eventData);
        }

        /// <summary>
        /// Called by the EventSystem every time the pointer is moved during dragging.
        /// </summary>
        public virtual void OnDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.Drag, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when an object accepts a drop.
        /// </summary>
        public virtual void OnDrop(PointerEventData eventData)
        {
            Execute(EventTriggerType.Drop, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a PointerDown event occurs.
        /// </summary>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerDown, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a PointerUp event occurs.
        /// </summary>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerUp, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a Click event occurs.
        /// </summary>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Execute(EventTriggerType.PointerClick, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a Select event occurs.
        /// </summary>
        public virtual void OnSelect(BaseEventData eventData)
        {
            Execute(EventTriggerType.Select, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a new object is being selected.
        /// </summary>
        public virtual void OnDeselect(BaseEventData eventData)
        {
            Execute(EventTriggerType.Deselect, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a new Scroll event occurs.
        /// </summary>
        public virtual void OnScroll(PointerEventData eventData)
        {
            Execute(EventTriggerType.Scroll, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a Move event occurs.
        /// </summary>
        public virtual void OnMove(AxisEventData eventData)
        {
            Execute(EventTriggerType.Move, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when the object associated with this EventTrigger is updated.
        /// </summary>
        public virtual void OnUpdateSelected(BaseEventData eventData)
        {
            Execute(EventTriggerType.UpdateSelected, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a drag has been found, but before it is valid to begin the drag.
        /// </summary>
        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.InitializePotentialDrag, eventData);
        }

        /// <summary>
        /// Called before a drag is started.
        /// </summary>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.BeginDrag, eventData);
        }

        /// <summary>
        /// Called by the EventSystem once dragging ends.
        /// </summary>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            Execute(EventTriggerType.EndDrag, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a Submit event occurs.
        /// </summary>
        public virtual void OnSubmit(BaseEventData eventData)
        {
            Execute(EventTriggerType.Submit, eventData);
        }

        /// <summary>
        /// Called by the EventSystem when a Cancel event occurs.
        /// </summary>
        public virtual void OnCancel(BaseEventData eventData)
        {
            Execute(EventTriggerType.Cancel, eventData);
        }
    }
}
