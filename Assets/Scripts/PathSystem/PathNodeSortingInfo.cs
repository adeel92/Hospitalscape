using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyAttributes;

namespace Isometric.PathSystem
{
    public class PathNodeSortingInfo : MonoBehaviour
    {
        [Serializable]
        private class SortingInfo
        {
            public PathNode ConnectedNode;
            [SortingLayer]
            public string SortingLayerName;
            public int OrderInLayer;
        }

        [SerializeField] List<SortingInfo> m_SortingInfos;

        public string GetSortingLayer(PathNode connectedNode)
        {
            SortingInfo sortingInfo = m_SortingInfos.Find((x) => x.ConnectedNode == connectedNode);
            if(sortingInfo != null)
            {
                return sortingInfo.SortingLayerName;
            }
            else
            {
                return null;
            }
        }

        public int? GetOrderInLayer(PathNode connectedNode)
        {
            SortingInfo sortingInfo = m_SortingInfos.Find((x) => x.ConnectedNode == connectedNode);
            if (sortingInfo != null)
            {
                return sortingInfo.OrderInLayer;
            }
            else
            {
                return null;
            }
        }
    }
}
