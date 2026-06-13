using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Isometric.Environment
{
    public class DoorAnimatorController : MonoBehaviour
    {
        private static readonly int IsOpen = Animator.StringToHash("IsOpen");

        [SerializeField] Animator m_DoorAnimator;


        public void OpenDoor()
        {
            m_DoorAnimator.SetBool(IsOpen, true);
        }

        public void CloseDoor()
        {
            m_DoorAnimator.SetBool(IsOpen, false);
        }
    }
}
