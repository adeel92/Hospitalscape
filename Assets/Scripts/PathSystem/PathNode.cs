using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;

namespace Isometric.PathSystem
{
    public class PathNode : MonoBehaviour
    {
        public PathController PathController => m_PathController;
        [SerializeField, ReadOnly] PathController m_PathController = null;

        public List<PathEdge> Edges => m_Edges;
        [SerializeField] List<PathEdge> m_Edges;

        public void Setup(PathController pathController)
        {
            m_PathController = pathController;
        }
    }

    [Serializable]
    public class PathEdge
    {
        public int Cost;
        public PathNode Node;
    }
}
