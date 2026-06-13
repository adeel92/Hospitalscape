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

        private static readonly int WorkLeft = Animator.StringToHash("WorkLeft");
        private static readonly int ServeDown = Animator.StringToHash("ServeDown");

        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");

        [SerializeField] Animator m_Animator;
        [SerializeField] float m_WalkSpeedMultiplier = 1;

        private PathDirection m_CurrentWalkingDirection = PathDirection.None;

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
                        m_CurrentWalkingDirection = PathDirection.Up;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(WalkUp);
                    }
                    break;
                case PathDirection.Down:
                    if (m_CurrentWalkingDirection != PathDirection.Down)
                    {
                        m_CurrentWalkingDirection = PathDirection.Down;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(WalkDown);
                    }
                    break;
                case PathDirection.Left:
                    if (m_CurrentWalkingDirection != PathDirection.Left)
                    {
                        m_CurrentWalkingDirection = PathDirection.Left;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(WalkLeft);
                    }
                    break;
                case PathDirection.Right:
                    if (m_CurrentWalkingDirection != PathDirection.Right)
                    {
                        m_CurrentWalkingDirection = PathDirection.Right;
                        m_Animator.SetFloat(WalkSpeed, walkSpeed * m_WalkSpeedMultiplier);
                        m_Animator.SetBool(Idle, false);
                        m_Animator.SetTrigger(WalkRight);
                    }
                    break;
            }
        }

        public void PlayWorkAnimation()
        {
            m_CurrentWalkingDirection = PathDirection.None;

            m_Animator.SetBool(Idle, false);
            m_Animator.SetTrigger(WorkLeft);
        }

        public void PlayServe()
        {
            m_Animator.SetBool(Idle, false);
            m_Animator.SetTrigger(ServeDown);
        }
    }
}
