using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.EventSystems
{
    // 使用物理光线投射的简单事件系统
    [AddComponentMenu("Event/Physics Raycaster")]
    [RequireComponent(typeof(Camera))]
    // Raycaster 用于针对3D物理组件进行铸造
    public class PhysicsRaycaster : BaseRaycaster
    {
        /// Const to use for clarity when no event mask is set
        // 事件的mask没有设置时，用于清晰显示的常量
        protected const int kNoEventMaskSet = -1;

        protected Camera m_EventCamera;

        /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
        // 层级遮罩用于筛选事件，如果使用摄像机，则始终与摄影机的剔除遮罩结合使用
        [SerializeField]
        protected LayerMask m_EventMask = kNoEventMaskSet;

        /// The max number of intersections allowed. 0 = allocating version anything else is non alloc.
        // 允许的最大交叉点数量，0=分配版本，其他任何内容都是非分配的
        [SerializeField]
        protected int m_MaxRayIntersections = 0;
        protected int m_LastMaxRayIntersections = 0;

        RaycastHit[] m_Hits;

        protected PhysicsRaycaster()
        {}

        public override Camera eventCamera
        {
            get
            {
                if (m_EventCamera == null)
                    m_EventCamera = GetComponent<Camera>();
                return m_EventCamera ?? Camera.main;
            }
        }


        // 用于确定事件处理顺序的深度
        public virtual int depth
        {
            get { return (eventCamera != null) ? (int)eventCamera.depth : 0xFFFFFF; }
        }

        // 用于确定哪些对象将接受事件的mask
        public int finalEventMask
        {
            get { return (eventCamera != null) ? eventCamera.cullingMask & m_EventMask : kNoEventMaskSet; }
        }

        // 用于筛选事件的事件遮罩，如果使用摄影机，则始终与摄影机的剔除遮罩结合使用
        public LayerMask eventMask
        {
            get { return m_EventMask; }
            set { m_EventMask = value; }
        }

        /// 允许找到的最大光线交点数
        /// 值为0表示使用光线投影函数的分配版本，而任何其他值都将使用非分配版本
        public int maxRayIntersections
        {
            get { return m_MaxRayIntersections; }
            set { m_MaxRayIntersections = value; }
        }

        /// 返回从摄影机穿过事件位置的光线以及该光线的近裁剪平面和远裁剪平面之间的距离
        /// </summary>
        /// <param name="eventData">The pointer event for which we will cast a ray.</param>
        /// <param name="ray">The ray to use.</param>
        /// <param name="distanceToClipPlane">The distance between the near and far clipping planes along the ray.</param>
        /// <returns>True if the operation was successful. false if it was not possible to compute, such as the eventPosition being outside of the view.</returns>
        protected bool ComputeRayAndDistance(PointerEventData eventData, ref Ray ray, ref float distanceToClipPlane)
        {
            if (eventCamera == null)
                return false;

            var eventPosition = Display.RelativeMouseAt(eventData.position);
            if (eventPosition != Vector3.zero)
            {
                // 我们支持多重显示和基于事件位置的显示标识
                int eventDisplayIndex = (int)eventPosition.z;

                // 放弃不在次显示中的事件，这样用户就不会同时与多个显示交互
                if (eventDisplayIndex != eventCamera.targetDisplay)
                    return false;
            }
            else
            {
                // 并非所有平台都支持多重显示系统，当不支持返回位置时
                // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
                // 将全部为0，因此当返回的索引为0时，为了安全起见，我们将默认使用事件数据
                eventPosition = eventData.position;
            }

            // 剔除视图矩形之外的光线投射 (case 636595)
            if (!eventCamera.pixelRect.Contains(eventPosition))
                return false;

            ray = eventCamera.ScreenPointToRay(eventPosition);
            // 补偿远平面距离 - see MouseEvents.cs
            float projectionDirection = ray.direction.z;
            distanceToClipPlane = Mathf.Approximately(0.0f, projectionDirection)
                ? Mathf.Infinity
                : Mathf.Abs((eventCamera.farClipPlane - eventCamera.nearClipPlane) / projectionDirection);
            return true;
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            Ray ray = new Ray();
            float distanceToClipPlane = 0;
            if (!ComputeRayAndDistance(eventData, ref ray, ref distanceToClipPlane))
                return;

            int hitCount = 0;

            if (m_MaxRayIntersections == 0)
            {
                if (ReflectionMethodsCache.Singleton.raycast3DAll == null)
                    return;

                m_Hits = ReflectionMethodsCache.Singleton.raycast3DAll(ray, distanceToClipPlane, finalEventMask);
                hitCount = m_Hits.Length;
            }
            else
            {
                if (ReflectionMethodsCache.Singleton.getRaycastNonAlloc == null)
                    return;

                if (m_LastMaxRayIntersections != m_MaxRayIntersections)
                {
                    m_Hits = new RaycastHit[m_MaxRayIntersections];
                    m_LastMaxRayIntersections = m_MaxRayIntersections;
                }

                hitCount = ReflectionMethodsCache.Singleton.getRaycastNonAlloc(ray, m_Hits, distanceToClipPlane, finalEventMask);
            }

            if (hitCount > 1)
                System.Array.Sort(m_Hits, (r1, r2) => r1.distance.CompareTo(r2.distance));

            if (hitCount != 0)
            {
                for (int b = 0, bmax = hitCount; b < bmax; ++b)
                {
                    var result = new RaycastResult
                    {
                        gameObject = m_Hits[b].collider.gameObject,
                        module = this,
                        distance = m_Hits[b].distance,
                        worldPosition = m_Hits[b].point,
                        worldNormal = m_Hits[b].normal,
                        screenPosition = eventData.position,
                        index = resultAppendList.Count,
                        sortingLayer = 0,
                        sortingOrder = 0
                    };
                    resultAppendList.Add(result);
                }
            }
        }
    }
}
