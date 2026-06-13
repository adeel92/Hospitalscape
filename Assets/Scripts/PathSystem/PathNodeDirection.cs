using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Isometric.PathSystem
{
    [RequireComponent(typeof(PathNode))]
    public class PathNodeDirection : MonoBehaviour
    {
        public List<PathDirectionData> DirectionData => m_DirectionData;
        [SerializeField] List<PathDirectionData> m_DirectionData;

#if UNITY_EDITOR
[ContextMenu("QuickNodeDataGet")]
    private void QuickNodeDataGet()
    {
        PathNode node = GetComponent<PathNode>();
        if (node)
        {
            // Record the current state of the object for undo purposes
            Undo.RecordObject(this, "QuickNodeDataGet");

            List<PathDirectionData> pathDirectionData = new List<PathDirectionData>();
            foreach (var item in node.Edges)
            {
                PathDirectionData temPathDirectionData = new PathDirectionData();
                temPathDirectionData.Node = item.Node;
                temPathDirectionData.Direction = PathDirection.None;

                pathDirectionData.Add(temPathDirectionData);
            }

            m_DirectionData = pathDirectionData;

            // Mark the object as dirty so changes are recognized by Unity
            EditorUtility.SetDirty(this);
        }
    }
#endif

    public PathDirection GetDirection(PathNode pathNode)
        {
            PathDirectionData pathDirectionData = m_DirectionData.Find((x) => x.Node == pathNode);
            if (pathDirectionData != null)
            {
                return pathDirectionData.Direction;
            }

            return PathDirection.None;
        }
    }
    
    [Serializable]
    public class PathDirectionData
    {
        public PathNode Node;
        public PathDirection Direction;
    }

    public enum PathDirection
    {
        None, Up, Down, Left, Right
    }
}
