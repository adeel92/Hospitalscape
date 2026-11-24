using Arc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isometric.UI
{
    public class StepFillBarInfo : MonoBehaviour
    {
        public GameObject Bar;
        public Image BarImage;

        [Space]
        [SerializeField] Sprite m_DefaultTag;
        [SerializeField] Sprite m_UpgradedTag;
        // [SerializeField] Image m_FillStartUpgradeTag;
        [SerializeField] Image m_FillEndUpgradeTag;
        
        [Space]
        public PlayDoTweenSequence BarPostActivationTween;


        public void OnNoUpgrade()
        {
            /* if(m_FillStartUpgradeTag != null)
            {
                m_FillStartUpgradeTag.sprite = m_UpgradedTag;
            } */

            if(m_FillEndUpgradeTag != null)
            {
                m_FillEndUpgradeTag.sprite = m_DefaultTag;
            }
        }

        public void OnUpgradeDone()
        {
            /* if(m_FillStartUpgradeTag != null)
            {
                m_FillStartUpgradeTag.sprite = m_UpgradedTag;
            } */

            if(m_FillEndUpgradeTag != null)
            {
                m_FillEndUpgradeTag.sprite = m_UpgradedTag;
            }
        }
    }
}
