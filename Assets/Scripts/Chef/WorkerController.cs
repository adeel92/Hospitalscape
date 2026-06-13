using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using NaughtyAttributes;
using Isometric.PathSystem;
using Isometric.Environment;

namespace Isometric.Worker
{
    public class WorkerController : MonoBehaviour
    {
        [Header("---Setup---")]
        [SerializeField] WorkerAnimatorController m_AnimatorController;
        [SerializeField] SortingGroup m_SortingGroup;
        [SerializeField] Transform m_OrderDropOriginPoint;
        private string m_DefaultSortingLayer;
        private int m_DefaultSortingOrder;

        [Header("---Paramaters---")]
        [SerializeField] float m_WalkSpeed;

        public float ServingDuration => m_ServingDuration;
        [SerializeField, ReadOnly] float m_ServingDuration;

        public int OrderCost => m_OrderCost;
        [SerializeField, ReadOnly] int m_OrderCost;

        private MainServiceController m_CurrentSeat = null;
        private SalonChairController m_CurrentSeatOld = null;
        private PathNode m_StartNode;

        public void Setup(PathNode startnode, float servingDuration, int OrderCost)
        {
            m_StartNode = startnode;
            m_ServingDuration = servingDuration;
            m_OrderCost = OrderCost;
            m_DefaultSortingLayer = m_SortingGroup.sortingLayerName;
            m_DefaultSortingOrder = m_SortingGroup.sortingOrder;
        }

        public void Serve(MainServiceController seat)
        {
            m_CurrentSeat = seat;
            PathTraverserExtension.MoveTarget(transform, m_StartNode, seat.WorkerServeNode, m_WalkSpeed, OnGoingToNodeEntering, OnReachedTable);
        }

        //mrcHefF
        public void Serve(SalonChairController seat)
        {
            m_CurrentSeatOld = seat;
            PathTraverserExtension.MoveTarget(transform, m_StartNode, seat.ServeNode, m_WalkSpeed, OnGoingToNodeEntering, OnReachedTable);
        }

        private void OnReachedTable(PathNode node)
        {
            m_AnimatorController.PlayServe();
            m_CurrentSeat.WorkerReachedMainServiceSeat(this, m_OrderDropOriginPoint.position);
        }

        public void ServeComplete()
        {
            PathTraverserExtension.MoveTarget(transform, m_CurrentSeat.WorkerServeNode, m_StartNode, m_WalkSpeed, OnGoingToNodeLeaving, OnReachedStartNode);
            m_CurrentSeat = null;
        }

        private void OnReachedStartNode(PathNode node)
        {
            m_AnimatorController.PlayIdle();
            WorkerManager.ServeComplete(this);
        }

        private void OnGoingToNodeEntering(PathNode node1, PathNode node2)
        {
            if (node1.TryGetComponent(out PathNodeDirection nodeDirection))
            {
                PathDirection direction = nodeDirection.GetDirection(node2);
                m_AnimatorController.PlayWalkAnimation(direction, m_WalkSpeed);
            }

            if (node2.TryGetComponent(out SortingGroup sortingGroup))
            {
                m_SortingGroup.sortingLayerName = sortingGroup.sortingLayerName;
                m_SortingGroup.sortingOrder = sortingGroup.sortingOrder;
            }
            else
            {
                m_SortingGroup.sortingLayerName = m_DefaultSortingLayer;
                m_SortingGroup.sortingOrder = m_DefaultSortingOrder;
            }

            WorkerManager.WorkerEnteredSalon(this, node2);
        }

        private void OnGoingToNodeLeaving(PathNode node1, PathNode node2)
        {
            if (node1.TryGetComponent(out PathNodeDirection nodeDirection))
            {
                PathDirection direction = nodeDirection.GetDirection(node2);
                m_AnimatorController.PlayWalkAnimation(direction, m_WalkSpeed);
            }

            if (node2.TryGetComponent(out SortingGroup sortingGroup))
            {
                m_SortingGroup.sortingLayerName = sortingGroup.sortingLayerName;
                m_SortingGroup.sortingOrder = sortingGroup.sortingOrder;
            }
            else
            {
                m_SortingGroup.sortingLayerName = m_DefaultSortingLayer;
                m_SortingGroup.sortingOrder = m_DefaultSortingOrder;
            }

            WorkerManager.WorkerLeavingSalon(this, node2);
        }

        public SortingGroup GetSortingGroup()
        {
            return m_SortingGroup;
        }

    }
}
