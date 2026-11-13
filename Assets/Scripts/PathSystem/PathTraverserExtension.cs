using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace Isometric.PathSystem
{
    /// <summary>
    /// Extended functionality to the PathController
    /// </summary>
    public class PathTraverserExtension
    {
        private class TargetData
        {
            public Transform Target;
            public Sequence Sequence;
            public bool ShouldStop = false;
            public float BaseSpeed = 1f;
        }

        private static List<TargetData> s_Targets = new List<TargetData>();

        /// <summary>
        /// Move target from nodeA to nodeB (Shortest Path)
        /// </summary>
        /// <param name="onTravelToNode"> Called when traveling to from node to next node
        /// <param name="onComplete"> Called when reaching the last possbile node
        /// Note (it will going to send the last reached node even if the traverse is canceled)
        public static void MoveTarget(Transform target, PathNode nodeA, PathNode nodeB, float speed,
                                        Action<PathNode, PathNode> onTravelToNode,
                                        Action<PathNode> onComplete)
        {
            if (nodeA.PathController == nodeB.PathController)
            {
                if (s_Targets.Find((x) => x.Target == target) == null)
                {
                    Sequence moveSequence = DOTween.Sequence();

                    TargetData targetData = new TargetData();
                    targetData.Target = target;
                    targetData.Sequence = moveSequence;
                    targetData.ShouldStop = false;
                    targetData.BaseSpeed = speed;
                    s_Targets.Add(targetData);

                    PathController pathController = nodeA.PathController;
                    List<PathNode> pathNodes = pathController.GetPath(nodeA, nodeB);

                    if (pathNodes.Count > 1)
                    {

                        for (int i = 1; i < pathNodes.Count; i++)
                        {
                            int temI = i;

                            Vector2 node1Position = pathNodes[i - 1].transform.position;
                            Vector2 node2Position = pathNodes[i].transform.position;

                            float duration = (Vector2.Distance(node1Position, node2Position)) / speed;

                            moveSequence.Append(
                            targetData.Target.DOMove(node2Position, duration)
                            .SetEase(Ease.Linear)
                            .OnStart(() =>
                            {
                                onTravelToNode?.Invoke(pathNodes[temI - 1], pathNodes[temI]);
                            })
                            .OnComplete(() =>
                            {
                                if (targetData.ShouldStop)
                                {
                                    s_Targets.Remove(targetData);
                                    moveSequence.Kill();
                                    onComplete?.Invoke(pathNodes[temI]);
                                }
                            }));
                        }
                    }

                    moveSequence.OnComplete(() =>
                    {
                        s_Targets.Remove(targetData);
                        onComplete?.Invoke(nodeB);
                    });

                }
                else
                {
                    Debug.LogWarning("Target is already traversing path");
                }
            }
            else
            {
                Debug.LogError("The nodes do not have the same PathController!");
            }
        }

        /// <summary>
        /// Move target from nodeA to nodeB (Shortest Path)
        /// </summary>
        /// <param name="onTravelToNode"> Called when traveling to from node to next node
        /// <param name="onComplete"> Called when reaching the last possbile node
        /// Note (it will going to send the last reached node even if the traverse is canceled)
        public static void MoveTarget(Transform target, PathNode nodeA, PathNode nodeB, float speed,
                                        Action<PathNode, PathNode> onTravelToNode,
                                        Action<float> onTravelToNodeDistance,
                                        Action<PathNode> onComplete)
        {
            if (nodeA.PathController == nodeB.PathController)
            {
                if (s_Targets.Find((x) => x.Target == target) == null)
                {
                    Sequence moveSequence = DOTween.Sequence();

                    TargetData targetData = new TargetData();
                    targetData.Target = target;
                    targetData.Sequence = moveSequence;
                    targetData.ShouldStop = false;
                    targetData.BaseSpeed = speed;
                    s_Targets.Add(targetData);

                    PathController pathController = nodeA.PathController;
                    List<PathNode> pathNodes = pathController.GetPath(nodeA, nodeB);

                    if (pathNodes.Count > 1)
                    {

                        for (int i = 1; i < pathNodes.Count; i++)
                        {
                            int temI = i;

                            Vector2 node1Position = pathNodes[i - 1].transform.position;
                            Vector2 node2Position = pathNodes[i].transform.position;

                            float distance = Vector2.Distance(node1Position, node2Position);
                            float duration = (distance) / speed;

                            moveSequence.Append(
                            targetData.Target.DOMove(node2Position, duration)
                            .SetEase(Ease.Linear)
                            .OnStart(() =>
                            {
                                onTravelToNode?.Invoke(pathNodes[temI - 1], pathNodes[temI]);
                                onTravelToNodeDistance?.Invoke(distance);
                            })
                            .OnComplete(() =>
                            {
                                if (targetData.ShouldStop)
                                {
                                    s_Targets.Remove(targetData);
                                    moveSequence.Kill();
                                    onComplete?.Invoke(pathNodes[temI]);
                                }
                            }));
                        }
                    }

                    moveSequence.OnComplete(() =>
                    {
                        s_Targets.Remove(targetData);
                        onComplete?.Invoke(nodeB);
                    });

                }
                else
                {
                    Debug.LogWarning("Target is already traversing path");
                }
            }
            else
            {
                Debug.LogError("The nodes do not have the same PathController!");
            }
        }

        public static void StopTargetImmediately(Transform target)
        {
            TargetData targetData = s_Targets.Find((x) => x.Target == target);
            if (targetData != null)
            {
                targetData.Sequence.Kill();
                s_Targets.Remove(targetData);
            }
        }

        public static void StopTarget(Transform target)
        {
            TargetData targetData = s_Targets.Find((x) => x.Target == target);
            if (targetData != null)
            {
                targetData.ShouldStop = true;
            }
        }

        /// <summary>
        /// Updates the movement speed of a target that is currently traversing a path.
        /// Works by rescaling the running DOTween Sequence's timeScale relative to the
        /// original speed it was started with, so remaining segments (including the one
        /// currently mid-flight) immediately speed up or slow down - no need to know
        /// node distances or restart the tween.
        /// </summary>
        /// <param name="target">The target whose speed should be updated (same Transform passed to MoveTarget)</param>
        /// <param name="newSpeed">The new speed, in the same units used for the original MoveTarget speed param</param>
        /// <returns>True if a traversing target was found and updated, false otherwise</returns>
        public static bool SetTargetSpeed(Transform target, float newSpeed)
        {
            TargetData targetData = s_Targets.Find((x) => x.Target == target);
            if (targetData == null)
            {
                Debug.LogWarning("Target is not currently traversing a path");
                return false;
            }

            if (newSpeed <= 0f)
            {
                Debug.LogWarning("New speed must be greater than 0");
                return false;
            }

            // timeScale is relative to the speed the sequence's durations were built with.
            // e.g. if the sequence was built at speed 2 and we want speed 4, timeScale = 2 (twice as fast).
            targetData.Sequence.timeScale = newSpeed / targetData.BaseSpeed;
            return true;
        }
    }
}