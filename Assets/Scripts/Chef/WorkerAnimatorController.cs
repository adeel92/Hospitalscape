using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric.PathSystem;

namespace Isometric.Worker
{
    public class WorkerAnimatorController : MonoBehaviour
    {
        private static readonly int WalkUp = Animator.StringToHash("WalkUp");
        private static readonly int WalkDown = Animator.StringToHash("WalkDown");
        private static readonly int WalkRight = Animator.StringToHash("WalkRight");
        private static readonly int WalkLeft = Animator.StringToHash("WalkLeft");

        private static readonly int IdleUp = Animator.StringToHash("IdleUp");
        private static readonly int IdleDown = Animator.StringToHash("IdleDown");
        private static readonly int IdleRight = Animator.StringToHash("IdleRight");
        private static readonly int IdleLeft = Animator.StringToHash("IdleLeft");

        private static readonly int WorkRight = Animator.StringToHash("WorkRight");
        private static readonly int ServeRight = Animator.StringToHash("ServeRight");

        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");

        [SerializeField] Animator m_Animator;
        [SerializeField] float m_WalkSpeedMultiplier = 1;

        private PathDirection m_CurrentWalkingDirection = PathDirection.None;
        private PathDirection m_CurrentIdleDirection = PathDirection.None;

        public void PlayIdle()
        {
            m_CurrentWalkingDirection = PathDirection.None;
            m_Animator.SetBool(Idle, true);
        }

        public void PlayWalkAnimation(PathDirection direction, float walkSpeed)
        {
            switch (direction)
            {
                case PathDirection.Up:
                    if (m_CurrentWalkingDirection != PathDirection.Up)
                    {
                        m_CurrentIdleDirection = PathDirection.None;
                        m_CurrentWalkingDirection = PathDirection.Up;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(WalkUp);
                    }
                    break;
                case PathDirection.Down:
                    if (m_CurrentWalkingDirection != PathDirection.Down)
                    {
                        m_CurrentIdleDirection = PathDirection.None;
                        m_CurrentWalkingDirection = PathDirection.Down;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(WalkDown);
                    }
                    break;
                case PathDirection.Left:
                    if (m_CurrentWalkingDirection != PathDirection.Left)
                    {
                        m_CurrentIdleDirection = PathDirection.None;
                        m_CurrentWalkingDirection = PathDirection.Left;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(WalkLeft);
                    }
                    break;
                case PathDirection.Right:
                    if (m_CurrentWalkingDirection != PathDirection.Right)
                    {
                        m_CurrentIdleDirection = PathDirection.None;
                        m_CurrentWalkingDirection = PathDirection.Right;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(WalkRight);
                    }
                    break;
            }
        }

        public void PlayIdleAnimation(PathDirection direction)
        {
            switch (direction)
            {
                case PathDirection.Up:
                    if (m_CurrentIdleDirection != PathDirection.Up)
                    {
                        m_CurrentWalkingDirection = PathDirection.None;
                        m_CurrentIdleDirection = PathDirection.Up;
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(IdleUp);
                    }
                    break;
                case PathDirection.Down:
                    if (m_CurrentIdleDirection != PathDirection.Down)
                    {
                        m_CurrentWalkingDirection = PathDirection.None;
                        m_CurrentIdleDirection = PathDirection.Down;
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(IdleDown);
                    }
                    break;
                case PathDirection.Left:
                    if (m_CurrentIdleDirection != PathDirection.Left)
                    {
                        m_CurrentWalkingDirection = PathDirection.None;
                        m_CurrentIdleDirection = PathDirection.Left;
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(IdleLeft);
                    }
                    break;
                case PathDirection.Right:
                    if (m_CurrentIdleDirection != PathDirection.Right)
                    {
                        m_CurrentWalkingDirection = PathDirection.None;
                        m_CurrentIdleDirection = PathDirection.Right;
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(IdleRight);
                    }
                    break;
            }
        }

        public void PlayWorkAnimation()
        {
            m_CurrentWalkingDirection = PathDirection.None;

            m_Animator.SetBool(Idle, false);
            m_Animator.SetTrigger(WorkRight);
        }

        public void PlayServe()
        {
            m_Animator.SetBool(Idle, false);
            m_Animator.SetTrigger(ServeRight);
        }
        public void PlayServeWait()
        {
            m_Animator.SetBool(Idle, false);
            m_Animator.SetTrigger(IdleRight);
        }
    }
}
