using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Isometric.PathSystem;

namespace Isometric.Player
{
    public class PlayerAnimatorController : MonoBehaviour
    {
        /*private static readonly int WorkUp = Animator.StringToHash("WorkUp");
        private static readonly int WorkDown = Animator.StringToHash("WorkDown");
        private static readonly int WorkRight = Animator.StringToHash("WorkRight");
        private static readonly int WorkLeft = Animator.StringToHash("WorkLeft");

        private static readonly int WalkUp = Animator.StringToHash("WalkUp");
        private static readonly int WalkDown = Animator.StringToHash("WalkDown");
        private static readonly int WalkRight = Animator.StringToHash("WalkRight");
        private static readonly int WalkLeft = Animator.StringToHash("WalkLeft");

        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int WhatCanIDo = Animator.StringToHash("WhatCanIDo");*/

        private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");

        [Serializable]
        private class AnimatorStateInfo
        {
            public PlayerAnimatorState AnimatorState;
            public string StateName;
        }


        [SerializeField] Animator m_Animator;
        [SerializeField] List<AnimatorStateInfo> m_AnimatorStatesInfo;
        [SerializeField] float m_WalkSpeedMultiplier = 1;

        private PathDirection m_CurrentWalkingDirection = PathDirection.None;

        public void PlayIdle()
        {
            m_CurrentWalkingDirection = PathDirection.None;
            m_Animator.Play(GetStateName(PlayerAnimatorState.Idle));
        }

        public void PlayWhatCanIdo()
        {
            m_CurrentWalkingDirection = PathDirection.None;
            m_Animator.Play(GetStateName(PlayerAnimatorState.WhatCanIDo));
        }

        public void PlayHappyWave()
        {
            m_CurrentWalkingDirection = PathDirection.None;
            m_Animator.Play(GetStateName(PlayerAnimatorState.HappyWave));
        }

        public void PlayHappyWon()
        {
            m_CurrentWalkingDirection = PathDirection.None;
            m_Animator.Play(GetStateName(PlayerAnimatorState.HappyWon));
        }

        public void PlaySadLost()
        {
            m_CurrentWalkingDirection = PathDirection.None;
            m_Animator.Play(GetStateName(PlayerAnimatorState.SadLost));
        }

        public void PlayWalkAnimation(PathDirection direction, float walkSpeed)
        {
            switch (direction)
            {
                case PathDirection.Up:
                    if (m_CurrentWalkingDirection != PathDirection.Up)
                    {
                        m_CurrentWalkingDirection = PathDirection.Up;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.Play(GetStateName(PlayerAnimatorState.WalkUp));
                    }
                    break;
                case PathDirection.Down:
                    if (m_CurrentWalkingDirection != PathDirection.Down)
                    {
                        m_CurrentWalkingDirection = PathDirection.Down;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.Play(GetStateName(PlayerAnimatorState.WalkDown));
                    }
                    break;
                case PathDirection.Left:
                    if (m_CurrentWalkingDirection != PathDirection.Left)
                    {
                        m_CurrentWalkingDirection = PathDirection.Left;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.Play(GetStateName(PlayerAnimatorState.WalkLeft));
                    }
                    break;
                case PathDirection.Right:
                    if (m_CurrentWalkingDirection != PathDirection.Right)
                    {
                        m_CurrentWalkingDirection = PathDirection.Right;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.Play(GetStateName(PlayerAnimatorState.WalkRight));
                    }
                    break;
            }
        }

        public void PlayWorkAnimation(PathDirection direction)
        {
            m_CurrentWalkingDirection = PathDirection.None;

            switch (direction)
            {
                case PathDirection.Up:
                    m_Animator.Play(GetStateName(PlayerAnimatorState.WorkUp));
                    break;
                case PathDirection.Down:
                    m_Animator.Play(GetStateName(PlayerAnimatorState.WorkDown));
                    break;
                case PathDirection.Left:
                    m_Animator.Play(GetStateName(PlayerAnimatorState.WorkLeft));
                    break;
                case PathDirection.Right:
                    m_Animator.Play(GetStateName(PlayerAnimatorState.WorkRight));
                    break;
            }
        }

        private string GetStateName(PlayerAnimatorState state)
        {
            AnimatorStateInfo stateInfo = m_AnimatorStatesInfo.Find(info => info.AnimatorState == state);
            return stateInfo?.StateName;
        }
    }

    public enum PlayerAnimatorState
    {
        Idle, 
        WhatCanIDo, 
        WalkUp, 
        WalkDown, 
        WalkRight, 
        WalkLeft,
        WorkUp,
        WorkDown,
        WorkRight,
        WorkLeft,
        HappyWave,
        HappyWon,
        SadLost
    }
}
