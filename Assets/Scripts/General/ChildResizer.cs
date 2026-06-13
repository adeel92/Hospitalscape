using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric
{
    public class ChildResizer : MonoBehaviour
    {
        [System.Serializable]
        private class ResizeInfo
        {
            public int NumberOfChidsCondition;
            public Vector3 Scale;
        }

        [SerializeField] bool m_CallOnStart;
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
        }
    }
}
