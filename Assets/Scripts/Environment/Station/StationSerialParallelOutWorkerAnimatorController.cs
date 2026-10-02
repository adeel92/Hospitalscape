using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Isometric.PathSystem;

namespace Isometric.Environment
{

    public class StationSerialParallelOutWorkerAnimatorController : MonoBehaviour
    {
        private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");


        [Serializable]
        private class AnimatorStateInfo
        {
            public StationSerialParallelOutChefAnimatorState AnimatorState;
            public string StateName;
        }

        [SerializeField] Animator m_Animator;
        [SerializeField] float m_WalkSpeedMultiplier = 1;
        [SerializeField] bool m_HoldWalk = false;
        [SerializeField] List<AnimatorStateInfo> m_AnimatorStatesInfo;

        private PathDirection m_CurrentWalkingDirection = PathDirection.None;

        public void PlayIdle()
        {
            m_CurrentWalkingDirection = PathDirection.None;
            m_Animator.Play(GetStateName(StationSerialParallelOutChefAnimatorState.Idle));
        }

        public void PlayWorkingUp()
        {
            m_CurrentWalkingDirection = PathDirection.None;
            m_Animator.Play(GetStateName(StationSerialParallelOutChefAnimatorState.WorkingUp));
        }

        public void PlayWalkAnimation(PathDirection direction, float walkSpeed)
        {
            switch (direction)
            {
                case PathDirection.Up:
                    if (m_CurrentWalkingDirection != PathDirection.Up)
                    {
                        m_CurrentWalkingDirection = PathDirection.Up;
                        m_Animator.SetFloat(WalkSpeed, GetWalkSpeedByIndex(walkSpeed));

                        if (m_HoldWalk)
                        {
                            m_Animator.Play(GetStateName(StationSerialParallelOutChefAnimatorState.WalkHoldingUp));
                        }
                        else
                        {
                            m_Animator.Play(GetStateName(StationSerialParallelOutChefAnimatorState.WalkUp));
                        }
                    }
                    break;
                case PathDirection.Down:
                    if (m_CurrentWalkingDirection != PathDirection.Down)
                    {
                        m_CurrentWalkingDirection = PathDirection.Down;
                        m_Animator.SetFloat(WalkSpeed, GetWalkSpeedByIndex(walkSpeed));

                        if (m_HoldWalk)
                        {
                            m_Animator.Play(GetStateName(StationSerialParallelOutChefAnimatorState.WalkHoldingDown));
                        }
                        else
                        {
                            m_Animator.Play(GetStateName(StationSerialParallelOutChefAnimatorState.WalkDown));
                        }
                    }
                    break;
                case PathDirection.Left:
                    if (m_CurrentWalkingDirection != PathDirection.Left)
                    {
                        m_CurrentWalkingDirection = PathDirection.Left;
                        m_Animator.SetFloat(WalkSpeed, GetWalkSpeedByIndex(walkSpeed));

                        if (m_HoldWalk)
                        {
                            m_Animator.Play(GetStateName(StationSerialParallelOutChefAnimatorState.WalkHoldingLeft));
                        }
                        else
                        {
                            m_Animator.Play(GetStateName(StationSerialParallelOutChefAnimatorState.WalkLeft));
                        }
                    }
                    break;
                case PathDirection.Right:
                    if (m_CurrentWalkingDirection != PathDirection.Right)
                    {
                        m_CurrentWalkingDirection = PathDirection.Right;
                        m_Animator.SetFloat(WalkSpeed, GetWalkSpeedByIndex(walkSpeed));

                        if (m_HoldWalk)
                        {
                            m_Animator.Play(GetStateName(StationSerialParallelOutChefAnimatorState.WalkHoldingRight));
                        }
                        else
                        {
                            m_Animator.Play(GetStateName(StationSerialParallelOutChefAnimatorState.WalkRight));
                        }
                    }
                    break;
            }
        }


        private float GetWalkSpeedByIndex(float walkSpeed)
        {
            return walkSpeed * m_WalkSpeedMultiplier;
        }


        private string GetStateName(StationSerialParallelOutChefAnimatorState state)
        {
            AnimatorStateInfo stateInfo = m_AnimatorStatesInfo.Find(info => info.AnimatorState == state);
            return stateInfo?.StateName;
        }

        public void SetHoldWalk(bool value)
        {
            m_HoldWalk = value;
        }
    }

    public enum StationSerialParallelOutChefAnimatorState
    {
        Idle,
        WalkUp,
        WalkDown,
        WalkRight,
        WalkLeft,
        WalkHoldingDown,
        WalkHoldingRight,
        WalkHoldingLeft,
        WalkHoldingUp,
        WorkingUp,
    }
}
