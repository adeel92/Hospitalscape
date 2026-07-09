using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Isometric
{
    public class ChildResizer : MonoBehaviour
    {
        [System.Serializable]
        private class ResizeInfo
        {
            public int NumberOfChidsCondition;
            public Vector3 Scale;
            public int LayoutLeftPadding;
            public float LayoutYSpacing;
        }

        [SerializeField] bool m_CallOnStart;
        [SerializeField] GridLayoutGroup m_GridLayoutGroup;
        [SerializeField] List<ResizeInfo> m_ResizeInfo;

        private void Start()
        {
            if(m_CallOnStart)
            {
                Resize();
            }
        }

        [ContextMenu("Resize")]
        public void Resize()
        {
            int numberOfChilds = 0;

            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                {
                    numberOfChilds++;
                }
            }

            ResizeInfo resizeInfo = m_ResizeInfo.Find((x) => x.NumberOfChidsCondition == numberOfChilds);
            if (resizeInfo != null)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).localScale = resizeInfo.Scale;
                }
            }

            m_GridLayoutGroup.padding.left = resizeInfo.LayoutLeftPadding;
            m_GridLayoutGroup.spacing = new Vector2(m_GridLayoutGroup.spacing.x, resizeInfo.LayoutYSpacing);
        }
    }
}
