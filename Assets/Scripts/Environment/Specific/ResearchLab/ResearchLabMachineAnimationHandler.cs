using System.Collections.Generic;
using UnityEngine;


namespace Isometric.Environment
{
    public class ResearchLabMachineAnimationHandler : MonoBehaviour
    {
        [Header("---Station Info---")]
        [SerializeField] StationSerialOrderInOutOnCustomerDemand m_StationSerialOrderInOutOnCustomerDemand;

        [Header("---Animation Handlers---")]
        [SerializeField] StationSerialOrderInOutItemAnimationHandler m_MachineAnimationHandler;
        [SerializeField] List<float> m_ProductionSpeedMultiplierValues = new();
        

        // OnDurationSetup
        public void SetProductionSpeed(int durationUpgradeIndex)
        {
            float speedMultiplier = m_ProductionSpeedMultiplierValues[durationUpgradeIndex];
            m_MachineAnimationHandler.SetProductionSpeed(speedMultiplier);
        }

        // OnDurationStart
        public void PlayProcess()
        {
            m_MachineAnimationHandler.PlayState(StationSerialOrderInOutItemAnimatorStates.Process, false, null, null);
        }

        // OnDurationComplete
        public void PlayInit()
        {
            m_MachineAnimationHandler.PlayState(StationSerialOrderInOutItemAnimatorStates.Init, false, null, null);
        }

        public void ResetAnimation()
        {
            m_MachineAnimationHandler.PlayState(StationSerialOrderInOutItemAnimatorStates.Init, true, null, null);
        }
    }
}