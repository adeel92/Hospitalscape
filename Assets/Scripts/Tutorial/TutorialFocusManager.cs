using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering;
using NaughtyAttributes;

namespace Isometric.Tutorial
{
    public class TutorialFocusManager : MonoBehaviour
    {
        private static TutorialFocusManager s_Instnace;

        [Serializable]
        private class FocusInfo
        {
            public FocusShapeType ShapeType;
            public TutorialSoftFocus TutorialFocusWithoutBorderPrefab;
            public TutorialSoftFocus TutorialFocusWithBorderPrefab;
            // public Sprite ShapeSprite;
        }

        [Serializable]
        private class OutputFocusDetail
        {
            public TutorialSoftFocus TutorialFocus;
            public Coroutine FocusCoroutine;
            // public bool HasExternalTween;
        }

        [SerializeField] GameObject m_Holder;
        [SerializeField] Image m_BackgroundImage;
        [SerializeField] Button m_BackgroundButton;
        [SerializeField] Transform m_FocusHolder;
        //[SerializeField] Image m_FocusImage;
        // [SerializeField] Image m_FocusImagePrefab;
        // public List<Tuple<Transform, Coroutine>> m_Focuses = new List<Tuple<Transform, Coroutine>>();
        [SerializeField, ReadOnly] List<OutputFocusDetail> m_Focuses = new();
        [SerializeField] List<FocusInfo> m_FocusInfos;

        [Header("---Background Tween Smoothness---")]
        [SerializeField] CanvasGroup m_CanvasGroup;
        [SerializeField] float m_FocusDisableDuration = 0.15f;
        [SerializeField] float m_BackgroundAlpha = 0.9f;
        [SerializeField] float m_BackgroundTweenDuration = 0.2f;

        /* [Header("---Focus Image Tween Smoothness---\n(Use this when external tween sequence for focus image is not present)")]
        [SerializeField] Ease m_FocusImageEaseType = Ease.InBack;
        [SerializeField] float m_FocusImageTweenDuration = 0.2f; */
        private Sequence m_FocusEnablingSequence = null;
        private Sequence m_FocusDisablingSequence = null;


        private void Awake()
        {
            if (s_Instnace == null)
            {
                s_Instnace = this;
            }
        }

        /// <summary>
        /// This soft mask focus displays the background directly without smoothness and keep the focus image at a fixed position
        /// </summary>
        public static Transform FocusAtPosition(Vector2 position, Vector2 offset, Vector2 scale, FocusShapeType shapeType, bool useBorderType = false)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return null;
            }

            s_Instnace.m_Holder.SetActive(true);

            FocusInfo focusInfo = s_Instnace.m_FocusInfos.Find((x) => x.ShapeType == shapeType);
            TutorialSoftFocus focusPrefab = useBorderType ? focusInfo.TutorialFocusWithBorderPrefab : focusInfo.TutorialFocusWithoutBorderPrefab;
            TutorialSoftFocus tutorialFocus = Instantiate(focusPrefab, s_Instnace.m_FocusHolder);
            // Image focusImage = tutorialFocus.FocusImage;

            Vector3 focusPosition = tutorialFocus.transform.position;
            focusPosition.x = position.x + offset.x;
            focusPosition.y = position.y + offset.y;

            tutorialFocus.transform.position = focusPosition;
            tutorialFocus.transform.localScale = scale;
            s_Instnace.m_CanvasGroup.alpha = 1f;

            /* if (focusInfo != null)
            {
                focusImage.sprite = focusInfo.ShapeSprite;
            } */

            OutputFocusDetail outputFocus = new()
            {
                TutorialFocus = tutorialFocus,
                FocusCoroutine = null,
                // HasExternalTween = false  
            };
            // Tuple<Transform, Coroutine> focus = new Tuple<Transform, Coroutine>(focusImage.transform, null);

            s_Instnace.m_Focuses.Add(outputFocus);
            tutorialFocus.PlayEnablingTween(null, null);

            return tutorialFocus.transform;
        }

        /// <summary>
        /// This soft mask focus uses DOTween to display the background with smoothness and keep the focus image at a fixed position
        /// </summary>
        public static Transform FocusAtPosition(Vector2 position, Vector2 offset, Vector2 scale, FocusShapeType shapeType, Action onFocusStart, Action onFocusComplete, bool useBorderType = false)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return null;
            }

            if(s_Instnace.m_FocusEnablingSequence != null)
            {
                s_Instnace.m_FocusEnablingSequence.Kill();    
            }

            s_Instnace.m_Holder.SetActive(true);

            FocusInfo focusInfo = s_Instnace.m_FocusInfos.Find((x) => x.ShapeType == shapeType);
            TutorialSoftFocus focusPrefab = useBorderType ? focusInfo.TutorialFocusWithBorderPrefab : focusInfo.TutorialFocusWithoutBorderPrefab;
            TutorialSoftFocus tutorialFocus = Instantiate(focusPrefab, s_Instnace.m_FocusHolder);
            // Image focusImage = tutorialFocus.FocusImage;

            Vector3 focusPosition = tutorialFocus.transform.position;
            focusPosition.x = position.x + offset.x;
            focusPosition.y = position.y + offset.y;

            tutorialFocus.transform.position = focusPosition;
            tutorialFocus.transform.localScale = scale;
            
            Color backgroundColor = s_Instnace.m_BackgroundImage.color;
            backgroundColor.a = 0f;
            s_Instnace.m_BackgroundImage.color = backgroundColor;
            s_Instnace.m_CanvasGroup.alpha = 1f;

            /* if (focusInfo != null)
            {
                focusImage.sprite = focusInfo.ShapeSprite;
            } */

            OutputFocusDetail outputFocus = new()
            {
                TutorialFocus = tutorialFocus,
                FocusCoroutine = null,
                // HasExternalTween = false  
            };
            // Tuple<Transform, Coroutine> focus = new Tuple<Transform, Coroutine>(focusImage.transform, null);

            s_Instnace.m_Focuses.Add(outputFocus);
            
            s_Instnace.m_FocusEnablingSequence = DOTween.Sequence();
            s_Instnace.m_FocusEnablingSequence.SetUpdate(true);

            /* s_Instnace.m_FocusEnablingSequence.InsertCallback(0,
                () => { onFocusStart?.Invoke(); }
            ); */

            s_Instnace.m_FocusEnablingSequence.Insert(0,
                s_Instnace.m_BackgroundImage.DOFade(s_Instnace.m_BackgroundAlpha, s_Instnace.m_BackgroundTweenDuration)
            );

            /* s_Instnace.m_FocusEnablingSequence.Insert(0,
                tutorialFocus.transform.DOScale(scale, s_Instnace.m_FocusImageTweenDuration).SetEase(s_Instnace.m_FocusImageEaseType)
            ); */

            /* s_Instnace.m_FocusEnablingSequence.OnComplete(() =>
            {
                onFocusComplete?.Invoke();
            }); */

            tutorialFocus.PlayEnablingTween(onFocusStart, onFocusComplete);

            return tutorialFocus.transform;
        }

        /// <summary>
        /// This soft mask focus displays the background directly without smoothness and let the focus image follow the target transform
        /// </summary>
        public static Transform FocusAtTransform(Transform target, Vector2 offset, Vector2 scale, FocusShapeType shapeType, bool useBorderType = false)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return null;
            }

            s_Instnace.m_Holder.SetActive(true);

            FocusInfo focusInfo = s_Instnace.m_FocusInfos.Find((x) => x.ShapeType == shapeType);
            TutorialSoftFocus focusPrefab = useBorderType ? focusInfo.TutorialFocusWithBorderPrefab : focusInfo.TutorialFocusWithoutBorderPrefab;
            TutorialSoftFocus tutorialFocus = Instantiate(focusPrefab, s_Instnace.m_FocusHolder);

            Vector3 focusPosition = tutorialFocus.transform.position;
            focusPosition.x = target.position.x + offset.x;
            focusPosition.y = target.position.y + offset.y;

            tutorialFocus.transform.position = focusPosition;
            tutorialFocus.transform.localScale = scale;
            s_Instnace.m_CanvasGroup.alpha = 1f;

            /* if (focusInfo != null)
            {
                focusImage.sprite = focusInfo.ShapeSprite;
            } */

            Coroutine focusCorotine = s_Instnace.StartCoroutine(s_Instnace.FocusingAtTransform(tutorialFocus.transform, target, offset));

            OutputFocusDetail outputFocus = new()
            {
                TutorialFocus = tutorialFocus,
                FocusCoroutine = focusCorotine,
                // HasExternalTween = false
            };
            // Tuple<Transform, Coroutine> focus = new Tuple<Transform, Coroutine>(focusImage.transform, focusCorotine);

            s_Instnace.m_Focuses.Add(outputFocus);
            tutorialFocus.PlayEnablingTween(null, null);

            return tutorialFocus.transform;
        }

        /// <summary>
        /// This soft mask focus uses DOTween to display the background with smoothness and let the focus image follow the target transform
        /// </summary>
        public static Transform FocusAtTransform(Transform target, Vector2 offset, Vector2 scale, FocusShapeType shapeType, Action onFocusStart, Action onFocusComplete, bool useBorderType = false)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return null;
            }

            if(s_Instnace.m_FocusEnablingSequence != null)
            {
                s_Instnace.m_FocusEnablingSequence.Kill();    
            }

            s_Instnace.m_Holder.SetActive(true);

            FocusInfo focusInfo = s_Instnace.m_FocusInfos.Find((x) => x.ShapeType == shapeType);
            TutorialSoftFocus focusPrefab = useBorderType ? focusInfo.TutorialFocusWithBorderPrefab : focusInfo.TutorialFocusWithoutBorderPrefab;
            TutorialSoftFocus tutorialFocus = Instantiate(focusPrefab, s_Instnace.m_FocusHolder);

            Vector3 focusPosition = tutorialFocus.transform.position;
            focusPosition.x = target.position.x + offset.x;
            focusPosition.y = target.position.y + offset.y;

            tutorialFocus.transform.position = focusPosition;
            tutorialFocus.transform.localScale = scale /* + (Vector2.one * 0.35f) */;

            Color backgroundColor = s_Instnace.m_BackgroundImage.color;
            backgroundColor.a = 0f;
            s_Instnace.m_BackgroundImage.color = backgroundColor;
            s_Instnace.m_CanvasGroup.alpha = 1f;

            /* if (focusInfo != null)
            {
                focusImage.sprite = focusInfo.ShapeSprite;
            } */

            Coroutine focusCorotine = s_Instnace.StartCoroutine(s_Instnace.FocusingAtTransform(tutorialFocus.transform, target, offset));

            OutputFocusDetail outputFocus = new()
            {
                TutorialFocus = tutorialFocus,
                FocusCoroutine = focusCorotine,
                // HasExternalTween = useExternalTween
            };
            // Tuple<Transform, Coroutine> focus = new Tuple<Transform, Coroutine>(focusImage.transform, focusCorotine);

            s_Instnace.m_Focuses.Add(outputFocus);
            
            s_Instnace.m_FocusEnablingSequence = DOTween.Sequence();
            s_Instnace.m_FocusEnablingSequence.SetUpdate(true);

            /* s_Instnace.m_FocusEnablingSequence.InsertCallback(0,
                () => { onFocusStart?.Invoke(); }
            ); */

            s_Instnace.m_FocusEnablingSequence.Insert(0,
                s_Instnace.m_BackgroundImage.DOFade(s_Instnace.m_BackgroundAlpha, s_Instnace.m_BackgroundTweenDuration)
            );

            /* s_Instnace.m_FocusEnablingSequence.Insert(0,
                tutorialFocus.transform.DOScale(scale, s_Instnace.m_FocusImageTweenDuration).SetEase(s_Instnace.m_FocusImageEaseType)
            ); */

            /* s_Instnace.m_FocusEnablingSequence.OnComplete(() =>
            {
                onFocusComplete?.Invoke();
            }); */

            tutorialFocus.PlayEnablingTween(onFocusStart, onFocusComplete);

            return tutorialFocus.transform;
        }

        public static void SetBackgroundButtonCallback(Action backgroundClickCallback)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instnace.m_BackgroundButton.onClick.RemoveAllListeners();
            s_Instnace.m_BackgroundButton.onClick.AddListener(() => backgroundClickCallback?.Invoke());
        }

        public static void RemoveBackgroundButtonCallback()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }
            s_Instnace.m_BackgroundButton.onClick.RemoveAllListeners();
        }

        public static void StopFocus(Transform focusTarget)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            var outputFocus = s_Instnace.m_Focuses.Find((x) => x.TutorialFocus == focusTarget);
            
            if(s_Instnace.m_FocusDisablingSequence != null)
            {
                s_Instnace.m_FocusDisablingSequence.Kill();
            }

            s_Instnace.m_FocusDisablingSequence = DOTween.Sequence();
            s_Instnace.m_FocusDisablingSequence.SetUpdate(true);
            s_Instnace.m_FocusDisablingSequence.Insert(0,
                s_Instnace.m_CanvasGroup.DOFade(0f, s_Instnace.m_FocusDisableDuration)
            );
            s_Instnace.m_FocusDisablingSequence.OnComplete(() =>
            {
                if (outputFocus != null)
                {
                    if (outputFocus.TutorialFocus != null)
                    {
                        Destroy(outputFocus.TutorialFocus.gameObject);
                    }

                    if (outputFocus.FocusCoroutine != null)
                    {
                        s_Instnace.StopCoroutine(outputFocus.FocusCoroutine);
                    }

                    s_Instnace.m_Focuses.Remove(outputFocus);
                }

                if (s_Instnace.m_Focuses.Count == 0)
                {
                    s_Instnace.m_Holder.SetActive(false);
                }
            });
        }

        public static void StopAllFocus()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            if(s_Instnace.m_FocusDisablingSequence != null)
            {
                s_Instnace.m_FocusDisablingSequence.Kill();
            }

            s_Instnace.m_FocusDisablingSequence = DOTween.Sequence();
            s_Instnace.m_FocusDisablingSequence.SetUpdate(true);
            s_Instnace.m_FocusDisablingSequence.Insert(0,
                s_Instnace.m_CanvasGroup.DOFade(0f, s_Instnace.m_FocusDisableDuration)
            );
            s_Instnace.m_FocusDisablingSequence.OnComplete(() =>
            {
                foreach (var outputFocus in s_Instnace.m_Focuses)
                {
                    if (outputFocus.TutorialFocus != null)
                    {
                        Destroy(outputFocus.TutorialFocus.gameObject);
                    }

                    if (outputFocus.FocusCoroutine != null)
                    {
                        s_Instnace.StopCoroutine(outputFocus.FocusCoroutine);
                    }
                }

                s_Instnace.m_Focuses = new();
                s_Instnace.m_Holder.SetActive(false);
            });
        }

        public static void StopAllFocusInstantly()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            if(s_Instnace.m_FocusDisablingSequence != null)
            {
                s_Instnace.m_FocusDisablingSequence.Kill();
            }

            s_Instnace.m_CanvasGroup.alpha = 0f;
            foreach (var outputFocus in s_Instnace.m_Focuses)
            {
                if (outputFocus.TutorialFocus != null)
                {
                    Destroy(outputFocus.TutorialFocus.gameObject);
                }

                if (outputFocus.FocusCoroutine != null)
                {
                    s_Instnace.StopCoroutine(outputFocus.FocusCoroutine);
                }
            }

            s_Instnace.m_Focuses = new();
            s_Instnace.m_Holder.SetActive(false);
        }


        IEnumerator FocusingAtTransform(Transform focus, Transform target, Vector2 offset)
        {
            while (true)
            {
                Vector3 focusPosition = focus.position;
                focusPosition.x = target.position.x + offset.x;
                focusPosition.y = target.position.y + offset.y;

                focus.position = focusPosition;

                yield return null;
            }
        }

        public static float GetFocusDisableDuration()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return 0f;
            }

            return s_Instnace.m_FocusDisableDuration;
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(TutorialFocusManager) + " is null");
        }
    }

    public enum FocusShapeType
    {
        Square, Circle, LongRectangle, ShortRectangle
    }
}
