using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Isometric.PathSystem
{
    /// <summary>
    /// Path System main controller consists of Node and connected by edges
    /// Can find the shortest path using this 
    /// </summary>
    public class PathController : MonoBehaviour
    {
        [SerializeField] List<PathNode> m_Nodes;

        private void Awake()
        {
            Setup();
        }

        public void Setup()
        {
            foreach (var node in m_Nodes)
            {
                node.Setup(this);
            }
        }

        public List<PathNode> GetPath(PathNode nodeA, PathNode nodeB)
        {
            var openSet = new SortedList<int, PathNode>();
            var nodeCosts = new Dictionary<PathNode, int>();
            var previousNodes = new Dictionary<PathNode, PathNode>();
            var visitedNodes = new HashSet<PathNode>();

            // Initialize distances
            foreach (var node in m_Nodes)
            {
                nodeCosts[node] = int.MaxValue;
            }
            nodeCosts[nodeA] = 0;

            // Add start node with a unique key
            openSet.Add(0, nodeA);

            // Process nodes in the priority queue
            while (openSet.Count > 0)
            {
                var currentNode = openSet.First().Value;
                var currentCost = openSet.First().Key;
                openSet.RemoveAt(0);

                if (currentNode == nodeB)
                {
                    return ReconstructPath(previousNodes, nodeB);
                }

                if (visitedNodes.Contains(currentNode))
                    continue;

                visitedNodes.Add(currentNode);

                foreach (var edge in currentNode.Edges)
                {
                    var neighbor = edge.Node;
                    if (visitedNodes.Contains(neighbor)) continue;

                    int newCost = currentCost + edge.Cost;

                    if (newCost < nodeCosts[neighbor])
                    {
                        nodeCosts[neighbor] = newCost;
                        previousNodes[neighbor] = currentNode;

                        // Handle duplicate key by appending an index
                        while (openSet.ContainsKey(newCost))
                        {
                            newCost++;
                        }

                        openSet.Add(newCost, neighbor);
                    }
                }
            }

            return null;  // No path found
        }

        private List<PathNode> ReconstructPath(Dictionary<PathNode, PathNode> previousNodes, PathNode endNode)
        {
            var path = new List<PathNode>();
            var currentNode = endNode;

            while (currentNode != null)
            {
                path.Add(currentNode);
                previousNodes.TryGetValue(currentNode, out currentNode);
            }

            path.Reverse();
            return path;
        }


#if UNITY_EDITOR

        [Header("---Visual---")]
        [SerializeField] bool m_ShowVisual = true;
        [SerializeField] bool m_ShowName = true;
        [SerializeField] Color m_VisualColor = Color.cyan;
        [SerializeField] float m_LineThickness = 2f;

        private void OnDrawGizmos()
        {
            if (!m_ShowVisual) return;

            if (m_Nodes == null || m_Nodes.Count == 0) return;

            // Use Gizmos for color setting
            Gizmos.color = m_VisualColor;
            foreach (var node in m_Nodes)
            {
                if (node == null || node.Edges == null) continue;

                foreach (var edge in node.Edges)
                {
                    if (edge.Node != null && HasEdge(edge.Node, node))
                    {
                        // Draw the line using Handles for thicker lines
                        Handles.color = m_VisualColor;
                        Handles.DrawAAPolyLine(m_LineThickness,
                            node.transform.position,
                            edge.Node.transform.position);
                    }
                }
            }

            // Draw nodes with Handles
            Handles.color = m_VisualColor;
            foreach (var node in m_Nodes)
            {
                if (node != null)
                {
                    Handles.DrawWireDisc(node.transform.position, Vector3.forward, 0.5f);

                    if(!m_ShowName)
                        continue;
                        
                    // Dynamic GUIStyle for label
                    var labelStyle = new GUIStyle()
                    {
                        fontStyle = FontStyle.Bold,
                        normal = new GUIStyleState { textColor = m_VisualColor }
                    };

                    Handles.Label(node.transform.position + Vector3.up * 0.3f, node.name, labelStyle);
                }
            }
        }

        private bool HasEdge(PathNode nodeA, PathNode nodeB)
        {
            foreach (var edge in nodeA.Edges)
            {
                if (edge.Node == nodeB)
                {
                    return true;
                }
            }
            return false;
        }

#endif
    }
}
