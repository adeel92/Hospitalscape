using System.Collections;
using UnityEngine;

namespace Isometric
{
    public class SpriteFillController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetSprite;

        private Material spriteMaterial;
        float elapsed = 0f;

        Coroutine m_FillSpriteOverTime = null;

        private void Awake()
        {
            if (targetSprite == null)
            {
                Debug.LogError("Target SpriteRenderer is not assigned.");
                return;
            }

            spriteMaterial = Instantiate(targetSprite.material);
            targetSprite.material = spriteMaterial;
        }

        public void SetFillAmount(float fillAmount)
        {
            elapsed = fillAmount;
            spriteMaterial.SetFloat("_FillAmount", fillAmount);
        }

        /// <summary>
        /// Fill from the 0 to 1 in the duration 
        /// </summary>
        public void StartFill(float duration)
        {
            if (m_FillSpriteOverTime != null)
            {
                StopCoroutine(m_FillSpriteOverTime);
                m_FillSpriteOverTime = null;
            }
            m_FillSpriteOverTime = StartCoroutine(FillSpriteOverTime(duration));
        }

        private IEnumerator FillSpriteOverTime(float duration)
        {
            spriteMaterial.SetFloat("_FillAmount", 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float fillAmount = Mathf.Clamp01(elapsed / duration);
                spriteMaterial.SetFloat("_FillAmount", fillAmount);
                yield return null;
            }

            spriteMaterial.SetFloat("_FillAmount", 1f);
        }

        public void StartFill(float fillAmountFrom, float fillAmountTo, float duration)
        {
            if (m_FillSpriteOverTime != null)
            {
                StopCoroutine(m_FillSpriteOverTime);
                m_FillSpriteOverTime = null;
            }
            m_FillSpriteOverTime = StartCoroutine(FillSpriteOverTime(fillAmountFrom, fillAmountTo, duration));
        }

        private IEnumerator FillSpriteOverTime(float fillAmountFrom, float fillAmountTo, float duration)
        {
            float elapsed = 0;
            
            spriteMaterial.SetFloat("_FillAmount", fillAmountFrom);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float fillAmount = Mathf.Lerp(fillAmountFrom, fillAmountTo, t); // Interpolate correctly
                spriteMaterial.SetFloat("_FillAmount", fillAmount);
                yield return null;
            }

            spriteMaterial.SetFloat("_FillAmount", fillAmountTo);
        }
    }
}
