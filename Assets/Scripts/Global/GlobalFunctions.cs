using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace Isometric
{
    public static class GlobalFunctions
    {

        /// <summary>
        /// Formate into million multiple to M and Billion to B
        /// </summary>
        public static string FormatNumberMB(this int num)
        {
            return FormatNumberMB((double)num);
        }

        /// <summary>
        /// Formate into million multiple to M and Billion to B
        /// </summary>
        public static string FormatNumberMB(this double num)
        {
            if (num >= 1_000_000_000) return (num / 1_000_000_000D).ToString("0.#") + "B";
            if (num >= 1_000_000) return (num / 1_000_000D).ToString("0.#") + "M";
            //if (num >= 1_000) return (num / 1_000D).ToString("0.#") + "K";

            return num.ToString("N0"); // Adds commas (e.g., 1,234)
        }


        public static string FormatTimeHHMMSS(this int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            return $"{hours:D2}:{minutes:D2}:{seconds:D2}"; // Ensures two digits (e.g., 01:05:09)
        }

        public static string FormatTimeMSS(this int totalSeconds)
        {
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            return $"{minutes}:{seconds:D2}";
        }

        public static string FormatSmartDurationShort(this double totalSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

            int seconds = time.Seconds;
            int minutes = time.Minutes;
            int hours = time.Hours;
            int days = time.Days;
            int weeks = days / 7;
            int months = weeks / 4; // Approximate: 4 weeks = 1 month

            if (months > 0)
            {
                int remWeeks = weeks % 4;
                return $"{months}mo, {remWeeks}w";
            }
            else if (weeks > 0)
            {
                int remDays = days % 7;
                return $"{weeks}w, {remDays}d";
            }
            else if (days > 0)
            {
                return $"{days}d, {hours}h";
            }
            else if (hours > 0)
            {
                return $"{hours}h, {minutes}m";
            }
            else
            {
                return $"{minutes}m, {seconds}s";
            }
        }

        public static Vector2 GetImageFillEndLocalPosition(Image image)
        {
            if (image == null) return Vector2.zero;

            RectTransform rectTransform = image.rectTransform;
            Vector2 size = rectTransform.rect.size;
            Vector2 pivotOffset = rectTransform.pivot * size; // Offset due to pivot
            Vector2 localEndPos = Vector2.zero;

            switch (image.fillMethod)
            {
                case Image.FillMethod.Horizontal:
                    localEndPos = new Vector2(
                        (image.fillAmount * size.x) * (image.fillOrigin == 1 ? -1 : 1),
                        0
                    );
                    localEndPos.x -= pivotOffset.x;
                    break;

                case Image.FillMethod.Vertical:
                    localEndPos = new Vector2(
                        0,
                        (image.fillAmount * size.y) * (image.fillOrigin == 1 ? -1 : 1)
                    );
                    localEndPos.y -= pivotOffset.y;
                    break;

                case Image.FillMethod.Radial360: // Assumes clockwise radial fill
                    float angle = image.fillAmount * 360f * Mathf.Deg2Rad;
                    localEndPos = new Vector2(
                        Mathf.Cos(angle) * size.x * 0.5f,
                        Mathf.Sin(angle) * size.y * 0.5f
                    );
                    localEndPos -= pivotOffset;
                    break;
            }

            return localEndPos;
        }


        public static void PlayAnimationWithCallback(MonoBehaviour caller, Animator animator, string animationName, Action onComplete)
        {
            caller.StartCoroutine(PlayAndCallback(animator, animationName, onComplete));
        }

        private static IEnumerator PlayAndCallback(Animator animator, string animationName, Action onComplete)
        {
            animator.Play(animationName);

            // Wait until the Animator is in the specified state
            yield return null; // wait one frame in case the transition hasn't happened yet

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            while (!state.IsName(animationName))
            {
                yield return null;
                state = animator.GetCurrentAnimatorStateInfo(0);
            }

            float time = state.length;

            if (animator.updateMode == AnimatorUpdateMode.Normal)
            {
                yield return new WaitForSeconds(time);
            }
            else
            {
                yield return new WaitForSecondsRealtime(time);
            }

            onComplete?.Invoke();
        }


        public static void PlayParticleWithCallback(MonoBehaviour caller, ParticleSystem particleSystem, Action onComplete)
        {
            if (particleSystem == null)
            {
                Debug.LogWarning("ParticleSystem is null");
                return;
            }

            particleSystem.Play();
            caller.StartCoroutine(WaitForParticle(particleSystem, onComplete));
        }

        private static IEnumerator WaitForParticle(ParticleSystem particleEffect, Action onComplete)
        {
            // Wait until the particle is finished
            while (particleEffect != null && particleEffect.IsAlive(true))
            {
                yield return null;
            }

            onComplete?.Invoke();
        }


        public static TweenerCore<Vector3, Vector3, VectorOptions> DoBounceScale(this Transform target, Vector3 startSize, Vector3 bounceSize, float duration)
        {
            target.DOKill();
            target.localScale = startSize;
            return target.DOScale(bounceSize, duration / 2).OnComplete(() =>
            {
                target.DOScale(startSize, duration / 2);
            });
        }


        public static void DeleteAllChildren(this Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        public static void TextCounter(this TextMeshProUGUI text, int countFrom, int countTo, float duration, MonoBehaviour monoBehaviour, bool timeScaled = true)
        {
            monoBehaviour.StartCoroutine(TextCounting(text, countFrom, countTo, duration, timeScaled));
        }

        private static IEnumerator TextCounting(TextMeshProUGUI text, int countFrom, int countTo, float duration, bool timeScaled)
        {
            float lerp = 0;

            while (lerp < duration)
            {
                text.text = Mathf.RoundToInt(Mathf.Lerp(countFrom, countTo, (lerp / (float)duration))).ToString();
                if (timeScaled)
                {
                    lerp += Time.deltaTime;
                }
                else
                {
                    lerp += Time.unscaledDeltaTime;
                }
                yield return null;
            }

            text.text = countTo.ToString();
        }

        public static List<int> GetShuffledRange(int minInclusive, int maxInclusive)
        {
            List<int> shuffledIndices = new List<int>();

            for (int i = minInclusive; i <= maxInclusive; i++)
            {
                shuffledIndices.Add(i);
            }

            int count = shuffledIndices.Count;
            for (int i = count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                int temp = shuffledIndices[i];
                shuffledIndices[i] = shuffledIndices[j];
                shuffledIndices[j] = temp;
            }

            return shuffledIndices;
        }

        public static void PlayAnimationWithCallbackUpdate(MonoBehaviour caller, Animator animator, string animationName, Action onComplete)
        {
            caller.StartCoroutine(PlayAndCallbackUpdate(animator, animationName, onComplete));
        }

        private static IEnumerator PlayAndCallbackUpdate(Animator animator, string animationName, Action onComplete)
        {
            animator.Play(animationName);
            yield return null;
            bool animationStarted = false;
            while (true)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

                if (state.IsName(animationName))
                {
                    animationStarted = true;

                    if (state.normalizedTime >= 1f && !animator.IsInTransition(0))
                    {
                        break;
                    }
                }
                else if (animationStarted)
                {
                    break;
                }

                yield return null;
            }

            onComplete?.Invoke();
        }
        
        public static Tweener DOOffsetMin(this RectTransform rt, Vector2 endValue, float duration)
        {
            return DOTween.To(() => rt.offsetMin, x => rt.offsetMin = x, endValue, duration);
        }

        public static Tweener DOOffsetMax(this RectTransform rt, Vector2 endValue, float duration)
        {
            return DOTween.To(() => rt.offsetMax, x => rt.offsetMax = x, endValue, duration);
        }
    }

}
