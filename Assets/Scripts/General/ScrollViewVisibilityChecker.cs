using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Arc
{
    [RequireComponent(typeof(RectTransform))]
    public class ScrollViewVisibilityChecker : MonoBehaviour
    {
        public bool IsVisible { get; private set; }

        public UnityEvent OnVisible;

        private ScrollRect m_ScrollRect;
        private RectTransform m_Viewport;
        private RectTransform m_ItemRect;

        private void Awake()
        {
            m_ItemRect = GetComponent<RectTransform>();
            m_ScrollRect = GetComponentInParent<ScrollRect>();

            if (m_ScrollRect != null)
            {
                m_Viewport = m_ScrollRect.viewport != null ? m_ScrollRect.viewport : m_ScrollRect.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogWarning("ScrollRect not found in parent hierarchy.");
            }
        }

        private void Update()
        {
            if (m_ScrollRect == null || m_Viewport == null)
                return;

            IsVisible = IsRectTransformInsideViewport(m_ItemRect, m_Viewport);


            if (IsVisible)
            {
                Debug.Log(gameObject.name);
                OnVisible?.Invoke();
            }
        }

        private bool IsRectTransformInsideViewport(RectTransform target, RectTransform container)
        {
            Vector3[] itemCorners = new Vector3[4];
            Vector3[] viewportCorners = new Vector3[4];

            target.GetWorldCorners(itemCorners);
            container.GetWorldCorners(viewportCorners);

            Rect itemWorldRect = new Rect(itemCorners[0], itemCorners[2] - itemCorners[0]);
            Rect viewportWorldRect = new Rect(viewportCorners[0], viewportCorners[2] - viewportCorners[0]);

            return itemWorldRect.Overlaps(viewportWorldRect, true);
        }
    }
}
