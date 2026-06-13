using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Arc
{
    public class PlayDoTween : MonoBehaviour
    {
        public enum CallType { OnStart, OnEnable, Manual }
        public enum TweenType { None, MoveTo, RotateTo, ScaleTo, AnchorMoveTo }

        [System.Serializable]
        public class CallBack
        {
            public enum CallbackTrigger { None, OnStart, OnUpdate, OnComplete, OnLoopComplete, OnReverseStart, OnReverseUpdate, OnReverseComplete, OnReverseLoopComplete }
            public CallbackTrigger callbackTrigger;
            public UnityEngine.Events.UnityEvent eventCallbacks;
        }

        public enum DurationType { time, speed }

        [Header("--Call Settings--")]
        public CallType callType;

        [System.Serializable]
        public class TweenInfo
        {
            public TweenType tweenType;
            public Ease easeType;
            public bool playFromStartValue = true;
            public Vector3 startTransformTweenValue;
            public Vector3 endTransformTweenValue;
            public bool isLocal = false;
        }
        [Header("--Transform Settings--")]
        public List<TweenInfo> tweens;


        [Header("--Fade Settings--")]
        public bool useFade;
        public Ease fadeTween;
        [Range(0, 1)]
        public float startFadeTweenValue = 0;
        [Range(0, 1)]
        public float endFadeTweenValue = 1;
        public bool useForAllChilds = false;

        [Header("--Tween Settings--")]
        public DurationType durationType;
        public float duration;
        public float delay;
        public bool shouldLoop = false;
        public LoopType loopType;
        public bool ignoreTimeScale;
        public List<CallBack> callbacks;

        private void Start()
        {
            if (callType == CallType.OnStart)
            {
                Play();
            }
        }

        private void OnEnable()
        {
            if (callType == CallType.OnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        Sequence tweenSequence = null;

        public void Play()
        {
            Stop();

            tweenSequence = DOTween.Sequence();

            foreach (var tween in tweens)
            {

                switch (tween.tweenType)
                {
                    case TweenType.MoveTo:
                        {
                            if (!tween.isLocal)
                            {
                                if (tween.playFromStartValue) transform.position = tween.startTransformTweenValue;
                                tweenSequence.Join(transform.DOMove(tween.endTransformTweenValue, duration).SetEase(tween.easeType));
                            }
                            else
                            {
                                if (tween.playFromStartValue) transform.localPosition = tween.startTransformTweenValue;
                                tweenSequence.Join(transform.DOLocalMove(tween.endTransformTweenValue, duration).SetEase(tween.easeType));
                            }

                            break;
                        }
                    case TweenType.RotateTo:
                        {
                            if (!tween.isLocal)
                            {
                                if (tween.playFromStartValue) transform.eulerAngles = tween.startTransformTweenValue;
                                tweenSequence.Join(transform.DORotate(tween.endTransformTweenValue, duration).SetEase(tween.easeType));
                            }
                            else
                            {
                                if (tween.playFromStartValue) transform.localEulerAngles = tween.startTransformTweenValue;
                                tweenSequence.Join(transform.DOLocalRotate(tween.endTransformTweenValue, duration).SetEase(tween.easeType));
                            }
                        }
                        break;
                    case TweenType.ScaleTo:
                        {
                            if (tween.playFromStartValue) transform.localScale = tween.startTransformTweenValue;
                            tweenSequence.Join(transform.DOScale(tween.endTransformTweenValue, duration).SetEase(tween.easeType));
                        }
                        break;
                    case TweenType.AnchorMoveTo:
                        {
                            RectTransform rectTransform = GetComponent<RectTransform>();
                            if (rectTransform)
                            {
                                if (tween.playFromStartValue) rectTransform.anchoredPosition = tween.startTransformTweenValue;
                                tweenSequence.Join(rectTransform.DOAnchorPos(tween.endTransformTweenValue, duration).SetEase(tween.easeType));
                            }
                        }
                        break;
                }
            }

            if (useFade)
            {
                if (useForAllChilds)
                {
                    foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
                    {
                        Color temColor = spriteRenderer.color;
                        temColor.a = startFadeTweenValue;
                        spriteRenderer.color = temColor;

                        tweenSequence.Join(spriteRenderer.DOFade(endFadeTweenValue, duration).SetEase(fadeTween));
                    }

                    foreach (var graphic in GetComponentsInChildren<Graphic>())
                    {
                        Color temColor = graphic.color;
                        temColor.a = startFadeTweenValue;
                        graphic.color = temColor;
                        tweenSequence.Join(graphic.DOFade(endFadeTweenValue, duration).SetEase(fadeTween));
                    }

                }
                else
                {
                    if (GetComponent<SpriteRenderer>())
                    {
                        Color temColor = GetComponent<SpriteRenderer>().color;
                        temColor.a = startFadeTweenValue;
                        GetComponent<SpriteRenderer>().color = temColor;
                        tweenSequence.Join(GetComponent<SpriteRenderer>().DOFade(endFadeTweenValue, duration).SetEase(fadeTween));
                    }

                    if (GetComponent<Graphic>())
                    {
                        Color temColor = GetComponent<Graphic>().color;
                        temColor.a = startFadeTweenValue;
                        GetComponent<Graphic>().color = temColor;
                        tweenSequence.Join(GetComponent<Graphic>().DOFade(endFadeTweenValue, duration).SetEase(fadeTween));
                    }
                }
            }

            tweenSequence.SetDelay(delay).SetSpeedBased(durationType == DurationType.speed ? true : false).SetLoops(shouldLoop ? -1 : 0, loopType).SetUpdate(ignoreTimeScale)
            .OnStart(() =>
            {
                foreach (var callback in callbacks) { if (callback.callbackTrigger == CallBack.CallbackTrigger.OnStart) { callback.eventCallbacks?.Invoke(); } }
            })
            .OnUpdate(() =>
            {
                foreach (var callback in callbacks) { if (callback.callbackTrigger == CallBack.CallbackTrigger.OnUpdate) { callback.eventCallbacks?.Invoke(); } }
            })
            .OnComplete(() =>
            {
                foreach (var callback in callbacks) { if (callback.callbackTrigger == CallBack.CallbackTrigger.OnComplete) { callback.eventCallbacks?.Invoke(); } }
            })
            .OnStepComplete(() =>
            {
                foreach (var callback in callbacks) { if (callback.callbackTrigger == CallBack.CallbackTrigger.OnLoopComplete) { callback.eventCallbacks?.Invoke(); } }
            });
        }

        public void PlayReverse()
        {
            Stop();

            tweenSequence = DOTween.Sequence();

            foreach (var tween in tweens)
            {

                switch (tween.tweenType)
                {
                    case TweenType.MoveTo:
                        {
                            if (!tween.isLocal)
                            {
                                if (tween.playFromStartValue) transform.position = tween.endTransformTweenValue;
                                tweenSequence.Join(transform.DOMove(tween.startTransformTweenValue, duration).SetEase(ReverseEaseType(tween.easeType)));
                            }
                            else
                            {
                                if (tween.playFromStartValue) transform.localPosition = tween.endTransformTweenValue;
                                tweenSequence.Join(transform.DOLocalMove(tween.startTransformTweenValue, duration).SetEase(ReverseEaseType(tween.easeType)));
                            }

                            break;
                        }
                    case TweenType.RotateTo:
                        {
                            if (!tween.isLocal)
                            {
                                if (tween.playFromStartValue) transform.eulerAngles = tween.endTransformTweenValue;
                                tweenSequence.Join(transform.DORotate(tween.startTransformTweenValue, duration).SetEase(ReverseEaseType(tween.easeType)));
                            }
                            else
                            {
                                if (tween.playFromStartValue) transform.localEulerAngles = tween.endTransformTweenValue;
                                tweenSequence.Join(transform.DOLocalRotate(tween.startTransformTweenValue, duration).SetEase(ReverseEaseType(tween.easeType)));
                            }
                        }
                        break;
                    case TweenType.ScaleTo:
                        {
                            if (tween.playFromStartValue) transform.localScale = tween.endTransformTweenValue;
                            tweenSequence.Join(transform.DOScale(tween.startTransformTweenValue, duration).SetEase(ReverseEaseType(tween.easeType)));
                        }
                        break;
                    case TweenType.AnchorMoveTo:
                        {
                            RectTransform rectTransform = GetComponent<RectTransform>();
                            if (rectTransform)
                            {
                                if (tween.playFromStartValue) rectTransform.anchoredPosition = tween.endTransformTweenValue;
                                tweenSequence.Join(rectTransform.DOAnchorPos(tween.startTransformTweenValue, duration).SetEase(ReverseEaseType(tween.easeType)));
                            }
                        }
                        break;
                }
            }

            if (useFade)
            {
                if (useForAllChilds)
                {
                    foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
                    {
                        Color temColor = spriteRenderer.color;
                        temColor.a = endFadeTweenValue;
                        spriteRenderer.color = temColor;

                        tweenSequence.Join(spriteRenderer.DOFade(startFadeTweenValue, duration).SetEase(fadeTween));
                    }

                    foreach (var graphic in GetComponentsInChildren<Graphic>())
                    {
                        Color temColor = graphic.color;
                        temColor.a = endFadeTweenValue;
                        graphic.color = temColor;
                        tweenSequence.Join(graphic.DOFade(startFadeTweenValue, duration).SetEase(fadeTween));
                    }

                }
                else
                {
                    if (GetComponent<SpriteRenderer>())
                    {
                        Color temColor = GetComponent<SpriteRenderer>().color;
                        temColor.a = endFadeTweenValue;
                        GetComponent<SpriteRenderer>().color = temColor;
                        tweenSequence.Join(GetComponent<SpriteRenderer>().DOFade(startFadeTweenValue, duration).SetEase(fadeTween));
                    }

                    if (GetComponent<Graphic>())
                    {
                        Color temColor = GetComponent<Graphic>().color;
                        temColor.a = endFadeTweenValue;
                        GetComponent<Graphic>().color = temColor;
                        tweenSequence.Join(GetComponent<Graphic>().DOFade(startFadeTweenValue, duration).SetEase(fadeTween));
                    }
                }
            }

            tweenSequence.SetDelay(delay).SetSpeedBased(durationType == DurationType.speed ? true : false).SetLoops(shouldLoop ? -1 : 0, loopType).SetUpdate(ignoreTimeScale)
            .OnStart(() =>
            {
                foreach (var callback in callbacks) { if (callback.callbackTrigger == CallBack.CallbackTrigger.OnReverseStart) { callback.eventCallbacks.Invoke(); } }
            })
            .OnUpdate(() =>
            {
                foreach (var callback in callbacks) { if (callback.callbackTrigger == CallBack.CallbackTrigger.OnReverseUpdate) { callback.eventCallbacks.Invoke(); } }
            })
            .OnComplete(() =>
            {
                foreach (var callback in callbacks) { if (callback.callbackTrigger == CallBack.CallbackTrigger.OnReverseComplete) { callback.eventCallbacks.Invoke(); } }
            })
            .OnStepComplete(() =>
            {
                foreach (var callback in callbacks) { if (callback.callbackTrigger == CallBack.CallbackTrigger.OnReverseLoopComplete) { callback.eventCallbacks.Invoke(); } }
            });
        }


        public void Stop()
        {
            if (tweenSequence != null)
            {
                tweenSequence.Kill();
                tweenSequence = null;
            }
        }

        public Ease ReverseEaseType(Ease easeType)
        {
            switch (easeType)
            {
                case Ease.Unset:
                    return Ease.Unset;
                case Ease.Linear:
                    return Ease.Linear;
                case Ease.InSine:
                    return Ease.OutSine;
                case Ease.OutSine:
                    return Ease.InSine;
                case Ease.InOutSine:
                    return Ease.InOutSine;
                case Ease.InQuad:
                    return Ease.OutQuad;
                case Ease.OutQuad:
                    return Ease.InQuad;
                case Ease.InOutQuad:
                    return Ease.InOutQuad;
                case Ease.InCubic:
                    return Ease.OutCubic;
                case Ease.OutCubic:
                    return Ease.InCubic;
                case Ease.InOutCubic:
                    return Ease.InOutCubic;
                case Ease.InQuart:
                    return Ease.OutQuart;
                case Ease.OutQuart:
                    return Ease.InQuart;
                case Ease.InOutQuart:
                    return Ease.InOutQuart;
                case Ease.InQuint:
                    return Ease.OutQuint;
                case Ease.OutQuint:
                    return Ease.InQuint;
                case Ease.InOutQuint:
                    return Ease.InOutQuint;
                case Ease.InExpo:
                    return Ease.OutExpo;
                case Ease.OutExpo:
                    return Ease.InExpo;
                case Ease.InOutExpo:
                    return Ease.InOutExpo;
                case Ease.InCirc:
                    return Ease.OutCirc;
                case Ease.OutCirc:
                    return Ease.InCirc;
                case Ease.InOutCirc:
                    return Ease.InOutCirc;
                case Ease.InElastic:
                    return Ease.OutElastic;
                case Ease.OutElastic:
                    return Ease.InElastic;
                case Ease.InOutElastic:
                    return Ease.InOutElastic;
                case Ease.InBack:
                    return Ease.OutBack;
                case Ease.OutBack:
                    return Ease.InBack;
                case Ease.InOutBack:
                    return Ease.InOutBack;
                case Ease.InBounce:
                    return Ease.OutBounce;
                case Ease.OutBounce:
                    return Ease.InBounce;
                case Ease.InOutBounce:
                    return Ease.InOutBounce;
                case Ease.Flash:
                    return Ease.Flash;
                case Ease.InFlash:
                    return Ease.OutFlash;
                case Ease.OutFlash:
                    return Ease.InFlash;
                case Ease.InOutFlash:
                    return Ease.InOutFlash;
                case Ease.INTERNAL_Zero:
                    return Ease.INTERNAL_Zero;
                case Ease.INTERNAL_Custom:
                    return Ease.INTERNAL_Custom;
            }
            return Ease.Unset;
        }


    }
}
