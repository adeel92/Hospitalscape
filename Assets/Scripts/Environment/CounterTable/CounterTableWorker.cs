using UnityEngine;

namespace Isometric.Environment
{
    public class CounterTableWorker : MonoBehaviour
    {
        [Space, SerializeField] CounterTableWorkerAnimatorController m_AnimatorController;
        [SerializeField] CounterTableController m_CounterController;


        public void Setup()
        {
            m_AnimatorController.PlayIdleLeft();
        }

        public void ResetToDefaultState()
        {
            m_AnimatorController.PlayIdleLeft();
        }
    }
}
