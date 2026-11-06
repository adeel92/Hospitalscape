using Isometric.Environment;
using Isometric.Tutorial;
using UnityEngine;

public class SimpleHintTestTubeStation : SimpleHintTarget
{
    [SerializeField] StationMainServiceInstantOrderOutOnCustomerDemand m_StationMainServiceInstantOrderOutOnCustomerDemand;

    public override void ShowHint(HintKey hintKey)
    {
        SimpleHintInfo simpleHintInfo = m_HintInfos.Find(x => x.HintKey == hintKey);
        
        if(simpleHintInfo == null)
        {
            Debug.LogWarning($"SimpleHintInfo with key '{hintKey}' does not exist in '{gameObject.name}'");
            return;
        }

        if (!m_StationMainServiceInstantOrderOutOnCustomerDemand.HasAnyDemandedCustomer())
        {
            return;
        }

        if (m_IsHiding)
        {
            return;
        }

        if (m_IsShowing)
        {
            CancelInvoke(nameof(HideHint));
            HideHint();
        }

        m_IsShowing = true;
        m_CurrentHintInfo =  simpleHintInfo;
        Invoke(nameof(EnableTargetHint), simpleHintInfo.ShowDelay);
    }
}
