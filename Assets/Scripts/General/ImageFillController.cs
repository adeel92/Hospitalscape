using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Isometric
{
    public class ImageFillController : MonoBehaviour
    {
        [SerializeField] private Image m_TargetImage;
        private Coroutine m_FillImageOverTime;
        private void Awake()
        {
            if (m_TargetImage == null)
            {
                Debug.LogError("Target Image is not assigned.");
            }
        }

        public void StartFill(float duration)
        {
            if (m_FillImageOverTime != null)
            {
                StopCoroutine(m_FillImageOverTime);
            }
            m_FillImageOverTime = StartCoroutine(FillImageOverTime(duration));
        }

        private IEnumerator FillImageOverTime(float duration)
        {
            float elapsed = 0f;
            m_TargetImage.fillAmount = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                m_TargetImage.fillAmount = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            m_TargetImage.fillAmount = 1f;
        }
    }
}
