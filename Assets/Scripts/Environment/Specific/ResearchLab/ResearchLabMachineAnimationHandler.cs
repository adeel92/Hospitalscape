using UnityEngine;


namespace Isometric.Environment
{
    public class ResearchLabMachineAnimationHandler : MonoBehaviour
    {
        [Header("---Station Info---")]
        [SerializeField] StationSerialOrderInOutOnCustomerDemand m_StationSerialOrderInOutOnCustomerDemand;

        [Header("---Animation Handlers---")]
        [SerializeField] StationSerialOrderInOutItemAnimationHandler m_MachineAnimationHandler;
        

        // OnDurationStart
        public void PlayProcess()
        {
            m_MachineAnimationHandler.PlayState(StationSerialOrderInOutItemAnimatorStates.Process, null, null);
        }

        // OnDurationComplete
        public void PlayInit()
        {
            m_MachineAnimationHandler.PlayState(StationSerialOrderInOutItemAnimatorStates.Init, null, null);
        }
    }
}