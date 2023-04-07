using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.EventSystems
{
    /// 使用物理光线投射的简单事件系统
    [AddComponentMenu("Event/Physics 2D Raycaster")]
    [RequireComponent(typeof(Camera))]
    // Raycaster 用于针对2D物理组件进行铸造
    public class Physics2DRaycaster : PhysicsRaycaster
    {
        RaycastHit2D[] m_Hits;

        protected Physics2DRaycaster()
        {}

        /// 对场景中的2D元素进行光线投射
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            Ray ray = new Ray();
            float distanceToClipPlane = 0;
            if (!ComputeRayAndDistance(eventData, ref ray, ref distanceToClipPlane))
                return;

            int hitCount = 0;

            if (maxRayIntersections == 0)
            {
                if (ReflectionMethodsCache.Singleton.getRayIntersectionAll == null)
                    return;

                m_Hits = ReflectionMethodsCache.Singleton.getRayIntersectionAll(ray, distanceToClipPlane, finalEventMask);
                hitCount = m_Hits.Length;
            }
            else
            {
                if (ReflectionMethodsCache.Singleton.getRayIntersectionAllNonAlloc == null)
                    return;

                if (m_LastMaxRayIntersections != m_MaxRayIntersections)
                {
                    m_Hits = new RaycastHit2D[maxRayIntersections];
                    m_LastMaxRayIntersections = m_MaxRayIntersections;
                }

                hitCount = ReflectionMethodsCache.Singleton.getRayIntersectionAllNonAlloc(ray, m_Hits, distanceToClipPlane, finalEventMask);
            }

            if (hitCount != 0)
            {
                for (int b = 0, bmax = hitCount; b < bmax; ++b)
                {
                    var sr = m_Hits[b].collider.gameObject.GetComponent<SpriteRenderer>();

                    var result = new RaycastResult
                    {
                        gameObject = m_Hits[b].collider.gameObject,
                        module = this,
                        distance = Vector3.Distance(eventCamera.transform.position, m_Hits[b].point),
                        worldPosition = m_Hits[b].point,
                        worldNormal = m_Hits[b].normal,
                        screenPosition = eventData.position,
                        index = resultAppendList.Count,
                        sortingLayer =  sr != null ? sr.sortingLayerID : 0,
                        sortingOrder = sr != null ? sr.sortingOrder : 0
                    };
                    resultAppendList.Add(result);
                }
            }
        }
    }
}
