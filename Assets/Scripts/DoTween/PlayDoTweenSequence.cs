using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

namespace Arc
{
    public class PlayDoTweenSequence : MonoBehaviour
    {
        public enum CallType { Manual, OnStart, OnEnable}
        public enum TweenInsertionType { Insert, Append, Join}
        public enum TweenType { Move, Rotate, Scale, AnchorMove, Fade, Callback, Interval, AnchorSizeDelta}
        public enum CallbackTrigger { OnStart, OnUpdate, OnComplete}

        [Serializable]
        public class TweenInfo
        {
            public TweenInsertionType insertionType;
            [ShowIf(nameof(insertionType), TweenInsertionType.Insert)]
            public float insertPostion;

            [Space]
            public TweenType tweenType;

            [ShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.Fade)]
            public Transform targetTransform;

            [ShowIf(nameof(tweenType), TweenType.AnchorMove,  TweenType.AnchorSizeDelta)]
            public RectTransform targetRectTransform;

            [ShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove, TweenType.Fade, TweenType.AnchorSizeDelta)]
            public Ease easeType;
            [ShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove, TweenType.Fade, TweenType.AnchorSizeDelta)]
            public float duration;
            [ShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove, TweenType.Fade,  TweenType.AnchorSizeDelta)]
            public float delay;

            [ShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove,  TweenType.AnchorSizeDelta)]
            public bool useStartTransformTweenValue = true;
            [ShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove,  TweenType.AnchorSizeDelta)]
            public Vector3 startTransformTweenValue;
            [ShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove,  TweenType.AnchorSizeDelta)]
            public Vector3 endTransformTweenValue;
            [ShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate)]
            public bool isLocal = false;

            [ShowIf(nameof(tweenType), TweenType.Fade)]
            public bool useStartFadeTweenValue = true;
            [ShowIf(nameof(tweenType), TweenType.Fade), Range(0, 1)]
            public float startFadeTweenValue = 0;
            [ShowIf(nameof(tweenType), TweenType.Fade), Range(0, 1)]
            public float endFadeTweenValue = 1;
            [ShowIf(nameof(tweenType), TweenType.Fade)]
            public bool useForAllChilds = false;

            [ShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove, TweenType.Fade,  TweenType.AnchorSizeDelta)]
            public CallBacksContainer tweenCallBacksContainer;

            [ShowIf(nameof(tweenType), TweenType.Callback)]
            public UnityEvent callback;

            [ShowIf(nameof(tweenType), TweenType.Interval)]
            public float interval;

        }

        [Serializable]
        public class CallBacksContainer
        {
            public List<CallBackInfo> CallBacksInfo;
        }

        [Serializable]
        public class CallBackInfo
        {
            public CallbackTrigger callbackTrigger;
            public UnityEvent Callback;
        }

        public CallType callType;
        public bool ignoreTimeScale;
        public bool killOnDisable = false;
        public List<TweenInfo> tweensInfo;
        public CallBacksContainer SequenceCallBacksInfo;


        Sequence tweenSequence = null;

        private void Start()
        {
            if (callType == CallType.OnStart)
            {
                PlaySequence();
            }
        }

        private void OnEnable()
        {
            if (callType == CallType.OnEnable)
            {
                PlaySequence();
            }
        }

        private void OnDisable()
        {
            if (killOnDisable)
            {
                Stop();
            }
        }

        public void PlaySequence()
        {
            PlaySequence(null);
        }

        public Sequence PlaySequence(Action onComplete)
        {
            Stop();

            tweenSequence = DOTween.Sequence();


            foreach (var tweenInfo in tweensInfo)
            {
                if (tweenInfo.tweenType == TweenType.Move)
                {
                    HandleMoveTweenInfo(tweenInfo, tweenSequence);
                }
                else if (tweenInfo.tweenType == TweenType.Rotate)
                {
                    HandleRotateTweenInfo(tweenInfo, tweenSequence);
                }
                else if (tweenInfo.tweenType == TweenType.Scale)
                {
                    HandleScaleTweenInfo(tweenInfo, tweenSequence);
                }
                else if (tweenInfo.tweenType == TweenType.AnchorMove)
                {
                    HandleAnchorMoveTweenInfo(tweenInfo, tweenSequence);
                }
                else if (tweenInfo.tweenType == TweenType.AnchorSizeDelta)
                {
                    HandleAnchorSizeDeltaTweenInfo(tweenInfo, tweenSequence);
                }
                else if (tweenInfo.tweenType == TweenType.Fade)
                {
                    HandleFadeTweenInfo(tweenInfo, tweenSequence);
                }
                else if (tweenInfo.tweenType == TweenType.Callback)
                {
                    HandleCallbackTweenInfo(tweenInfo, tweenSequence);
                }
                else if (tweenInfo.tweenType == TweenType.Interval)
                {
                    HandleIntervalTweenInfo(tweenInfo, tweenSequence);
                }
            }

            tweenSequence.SetUpdate(ignoreTimeScale)
            .OnStart(() =>
            {
                if (SequenceCallBacksInfo != null && SequenceCallBacksInfo.CallBacksInfo != null)
                {
                    foreach (var callback in SequenceCallBacksInfo.CallBacksInfo) { if (callback.callbackTrigger == CallbackTrigger.OnStart) { callback.Callback?.Invoke(); } }
                }
            })
            .OnUpdate(() =>
            {
                if (SequenceCallBacksInfo != null && SequenceCallBacksInfo.CallBacksInfo != null)
                {
                    foreach (var callback in SequenceCallBacksInfo.CallBacksInfo) { if (callback.callbackTrigger == CallbackTrigger.OnUpdate) { callback.Callback?.Invoke(); } }
                }
            })
            .OnComplete(() =>
            {
                if (SequenceCallBacksInfo != null && SequenceCallBacksInfo.CallBacksInfo != null)
                {
                    foreach (var callback in SequenceCallBacksInfo.CallBacksInfo) { if (callback.callbackTrigger == CallbackTrigger.OnComplete) { callback.Callback?.Invoke(); } }
                }

                onComplete?.Invoke();
            });

            return tweenSequence;
        }

        public void Stop()
        {
            if (tweenSequence != null)
            {
                tweenSequence.Kill();
                tweenSequence = null;
            }
        }

        private void HandleMoveTweenInfo(TweenInfo tweenInfo, Sequence sequence)
        {
            Transform target = tweenInfo.targetTransform;

            if (target == null)
            {
                DebugTarget();
                return;
            }

            if (tweenInfo.isLocal == false)
            {
                //InsertCallback(tweenInfo, () => { if (tweenInfo.useStartTransformTweenValue) target.position = tweenInfo.startTransformTweenValue; }, sequence);
                if (tweenInfo.useStartTransformTweenValue) target.position = tweenInfo.startTransformTweenValue;
                Tween movetween = target.DOMove(tweenInfo.endTransformTweenValue, tweenInfo.duration)
                    .SetEase(tweenInfo.easeType)
                    .SetDelay(tweenInfo.delay);
                HandleTweenCallback(tweenInfo, movetween);
                HandleSequenceInsertion(tweenInfo, movetween, sequence);
            }
            else
            {
                //InsertCallback(tweenInfo, () => { if (tweenInfo.useStartTransformTweenValue) target.localPosition = tweenInfo.startTransformTweenValue; }, sequence);
                if (tweenInfo.useStartTransformTweenValue) target.localPosition = tweenInfo.startTransformTweenValue;
                Tween movetween = target.DOLocalMove(tweenInfo.endTransformTweenValue, tweenInfo.duration)
                    .SetEase(tweenInfo.easeType)
                    .SetDelay(tweenInfo.delay);
                HandleTweenCallback(tweenInfo, movetween);
                HandleSequenceInsertion(tweenInfo, movetween, sequence);
            }
        }

        private void HandleRotateTweenInfo(TweenInfo tweenInfo, Sequence sequence)
        {
            Transform target = tweenInfo.targetTransform;

            if (target == null)
            {
                DebugTarget();
                return;
            }

            if (tweenInfo.isLocal == false)
            {
                //InsertCallback(tweenInfo, () => { if (tweenInfo.useStartTransformTweenValue) target.eulerAngles = tweenInfo.startTransformTweenValue; }, sequence);
                if (tweenInfo.useStartTransformTweenValue) target.eulerAngles = tweenInfo.startTransformTweenValue;
                Tween movetween = target.DORotate(tweenInfo.endTransformTweenValue, tweenInfo.duration)
                    .SetEase(tweenInfo.easeType)
                    .SetDelay(tweenInfo.delay);
                HandleTweenCallback(tweenInfo, movetween);
                HandleSequenceInsertion(tweenInfo, movetween, sequence);
            }
            else
            {
                //InsertCallback(tweenInfo, () => { if (tweenInfo.useStartTransformTweenValue) target.localEulerAngles = tweenInfo.startTransformTweenValue; }, sequence);
                if (tweenInfo.useStartTransformTweenValue) target.localEulerAngles = tweenInfo.startTransformTweenValue;
                Tween movetween = target.DOLocalRotate(tweenInfo.endTransformTweenValue, tweenInfo.duration)
                    .SetEase(tweenInfo.easeType)
                    .SetDelay(tweenInfo.delay);
                HandleTweenCallback(tweenInfo, movetween);
                HandleSequenceInsertion(tweenInfo, movetween, sequence);
            }
        }

        private void HandleScaleTweenInfo(TweenInfo tweenInfo, Sequence sequence)
        {
            Transform target = tweenInfo.targetTransform;

            if (target == null)
            {
                DebugTarget();
                return;
            }

            //InsertCallback(tweenInfo, () => { if (tweenInfo.useStartTransformTweenValue) target.localScale = tweenInfo.startTransformTweenValue; }, sequence);
            if (tweenInfo.useStartTransformTweenValue) target.localScale = tweenInfo.startTransformTweenValue;
            Tween movetween = target.DOScale(tweenInfo.endTransformTweenValue, tweenInfo.duration)
                .SetEase(tweenInfo.easeType)
                .SetDelay(tweenInfo.delay);
            HandleTweenCallback(tweenInfo, movetween);
            HandleSequenceInsertion(tweenInfo, movetween, sequence);
        }

        private void HandleAnchorMoveTweenInfo(TweenInfo tweenInfo, Sequence sequence)
        {
            RectTransform target = tweenInfo.targetRectTransform;

            if (target == null)
            {
                DebugTarget();
                return;
            }

            if (tweenInfo.useStartTransformTweenValue) target.anchoredPosition = tweenInfo.startTransformTweenValue;
            Tween movetween = target.DOAnchorPos(tweenInfo.endTransformTweenValue, tweenInfo.duration)
                .SetEase(tweenInfo.easeType)
                .SetDelay(tweenInfo.delay);
            HandleTweenCallback(tweenInfo, movetween);
            HandleSequenceInsertion(tweenInfo, movetween, sequence);
        }

         private void HandleAnchorSizeDeltaTweenInfo(TweenInfo tweenInfo, Sequence sequence)
        {
            RectTransform target = tweenInfo.targetRectTransform;

            if (target == null)
            {
                DebugTarget();
                return;
            }

            if (tweenInfo.useStartTransformTweenValue) target.sizeDelta = tweenInfo.startTransformTweenValue;
            Tween movetween = target.DOSizeDelta(tweenInfo.endTransformTweenValue, tweenInfo.duration)
                .SetEase(tweenInfo.easeType)
                .SetDelay(tweenInfo.delay);
            HandleTweenCallback(tweenInfo, movetween);
            HandleSequenceInsertion(tweenInfo, movetween, sequence);
        }

        private void HandleFadeTweenInfo(TweenInfo tweenInfo, Sequence sequence)
        {
            Transform target = tweenInfo.targetTransform;

            if (target == null)
            {
                DebugTarget();
                return;
            }

            if (tweenInfo.useForAllChilds)
            {
                bool hasGraphics = false;
                bool isFirst = true;
                foreach (var graphic in target.GetComponentsInChildren<Graphic>())
                {
                    if (tweenInfo.useStartFadeTweenValue)
                    {
                        Color temColor = graphic.color;
                        temColor.a = tweenInfo.startFadeTweenValue;
                        graphic.color = temColor;
                    }

                    Tween tweenFade = graphic.DOFade(tweenInfo.endFadeTweenValue, tweenInfo.duration)
                        .SetEase(tweenInfo.easeType)
                        .SetDelay(tweenInfo.delay);
                    HandleTweenCallback(tweenInfo, tweenFade);
                    if (isFirst)
                    {
                        /*InsertCallback(tweenInfo, () =>
                        {
                            if (tweenInfo.useStartFadeTweenValue)
                            {
                                Color temColor = graphic.color;
                                temColor.a = tweenInfo.startFadeTweenValue;
                                graphic.color = temColor;
                            }
                        }, sequence);*/

                        HandleSequenceInsertion(tweenInfo, tweenFade, sequence);
                        isFirst = false;
                    }
                    else
                    {
                        /*InsertCallbackMultiple(() =>
                        {
                            if (tweenInfo.useStartFadeTweenValue)
                            {
                                Color temColor = graphic.color;
                                temColor.a = tweenInfo.startFadeTweenValue;
                                graphic.color = temColor;
                            }
                        }, sequence);*/
                        HandleSequenceInsertionMultiple(tweenFade, sequence);
                    }

                    hasGraphics = true;
                }

                if (hasGraphics == false)
                {
                    isFirst = true;
                    foreach (var spriteRenderer in target.GetComponentsInChildren<SpriteRenderer>())
                    {
                        if (tweenInfo.useStartFadeTweenValue)
                        {
                            Color temColor = spriteRenderer.color;
                            temColor.a = tweenInfo.startFadeTweenValue;
                            spriteRenderer.color = temColor;
                        }

                        Tween tweenFade = spriteRenderer.DOFade(tweenInfo.endFadeTweenValue, tweenInfo.duration)
                            .SetEase(tweenInfo.easeType)
                            .SetDelay(tweenInfo.delay);
                        HandleTweenCallback(tweenInfo, tweenFade);
                        if (isFirst)
                        {
                            /*InsertCallback(tweenInfo, () =>
                            {
                                if (tweenInfo.useStartFadeTweenValue)
                                {
                                    Color temColor = spriteRenderer.color;
                                    temColor.a = tweenInfo.startFadeTweenValue;
                                    spriteRenderer.color = temColor;
                                }
                            }, sequence);*/
                            HandleSequenceInsertion(tweenInfo, tweenFade, sequence);
                            isFirst = false;
                        }
                        else
                        {
                            /*InsertCallbackMultiple(() =>
                            {
                                if (tweenInfo.useStartFadeTweenValue)
                                {
                                    Color temColor = spriteRenderer.color;
                                    temColor.a = tweenInfo.startFadeTweenValue;
                                    spriteRenderer.color = temColor;
                                }
                            }, sequence);*/
                            HandleSequenceInsertionMultiple(tweenFade, sequence);
                        }
                    }
                }
            }
            else
            {
                if (target.TryGetComponent(out CanvasGroup canvasGroup))
                {
                    if (tweenInfo.useStartFadeTweenValue)
                    {
                        canvasGroup.alpha = tweenInfo.startFadeTweenValue;
                    }

                    Tween tweenFade = canvasGroup.DOFade(tweenInfo.endFadeTweenValue, tweenInfo.duration)
                        .SetEase(tweenInfo.easeType)
                        .SetDelay(tweenInfo.delay);
                    /*InsertCallback(tweenInfo, () =>
                    {
                        if (tweenInfo.useStartFadeTweenValue)
                        {
                            canvasGroup.alpha = tweenInfo.startFadeTweenValue;
                        }
                    }, sequence);*/
                    HandleTweenCallback(tweenInfo, tweenFade);
                    HandleSequenceInsertion(tweenInfo, tweenFade, sequence);
                }
                else if (target.TryGetComponent(out Graphic graphic))
                {
                    if (tweenInfo.useStartFadeTweenValue)
                    {
                        Color temColor = graphic.color;
                        temColor.a = tweenInfo.startFadeTweenValue;
                        graphic.color = temColor;
                    }

                    Tween tweenFade = graphic.DOFade(tweenInfo.endFadeTweenValue, tweenInfo.duration)
                        .SetEase(tweenInfo.easeType)
                        .SetDelay(tweenInfo.delay);
                    /*InsertCallback(tweenInfo, () =>
                    {
                        if (tweenInfo.useStartFadeTweenValue)
                        {
                            Color temColor = graphic.color;
                            temColor.a = tweenInfo.startFadeTweenValue;
                            graphic.color = temColor;
                        }
                    }, sequence);*/
                    HandleTweenCallback(tweenInfo, tweenFade);
                    HandleSequenceInsertion(tweenInfo, tweenFade, sequence);
                }
                else if (target.TryGetComponent(out SpriteRenderer spriteRenderer))
                {
                    if (tweenInfo.useStartFadeTweenValue)
                    {
                        Color temColor = spriteRenderer.color;
                        temColor.a = tweenInfo.startFadeTweenValue;
                        spriteRenderer.color = temColor;
                    }

                    Tween tweenFade = spriteRenderer.DOFade(tweenInfo.endFadeTweenValue, tweenInfo.duration)
                        .SetEase(tweenInfo.easeType)
                        .SetDelay(tweenInfo.delay);
                    /*InsertCallback(tweenInfo, () =>
                    {
                        if (tweenInfo.useStartFadeTweenValue)
                        {
                            Color temColor = spriteRenderer.color;
                            temColor.a = tweenInfo.startFadeTweenValue;
                            spriteRenderer.color = temColor;
                        }
                    }, sequence);*/
                    HandleTweenCallback(tweenInfo, tweenFade);
                    HandleSequenceInsertion(tweenInfo, tweenFade, sequence);
                }
            }
        }


        private void HandleCallbackTweenInfo(TweenInfo tweenInfo, Sequence sequence)
        {
            if (tweenInfo.insertionType == TweenInsertionType.Insert)
            {
                sequence.InsertCallback(tweenInfo.insertPostion, () => tweenInfo.callback?.Invoke());
            }
            else if (tweenInfo.insertionType == TweenInsertionType.Append)
            {
                sequence.AppendCallback(() => tweenInfo.callback?.Invoke());
            }
            else
            {
                sequence.JoinCallback(() => tweenInfo.callback?.Invoke());
            }
        }

        private void InsertCallback(TweenInfo tweenInfo, Action callback, Sequence sequence)
        {
            if (tweenInfo.insertionType == TweenInsertionType.Insert)
            {
                sequence.InsertCallback(tweenInfo.insertPostion, () => callback?.Invoke());
            }
            else if (tweenInfo.insertionType == TweenInsertionType.Append)
            {
                sequence.AppendCallback(() => callback?.Invoke());
            }
            else
            {
                sequence.JoinCallback(() => callback?.Invoke());
            }
        }


        private void InsertCallbackMultiple(Action callback, Sequence sequence)
        {
            sequence.JoinCallback(() => callback?.Invoke());
        }

        private void HandleIntervalTweenInfo(TweenInfo tweenInfo, Sequence sequence)
        {
            sequence.AppendInterval(tweenInfo.interval);
        }

        private void HandleTweenCallback(TweenInfo tweenInfo, Tween tween)
        {
            tween.OnStart(() =>
            {
                if (tweenInfo.tweenCallBacksContainer != null && tweenInfo.tweenCallBacksContainer.CallBacksInfo != null)
                {
                    foreach (var callbackInfo in tweenInfo.tweenCallBacksContainer.CallBacksInfo)
                    {
                        if (callbackInfo.callbackTrigger == CallbackTrigger.OnStart)
                        {
                            callbackInfo.Callback?.Invoke();
                        }
                    }
                }
            })
            .OnUpdate(() =>
            {
                if (tweenInfo.tweenCallBacksContainer != null && tweenInfo.tweenCallBacksContainer.CallBacksInfo != null)
                {
                    foreach (var callbackInfo in tweenInfo.tweenCallBacksContainer.CallBacksInfo)
                    {
                        if (callbackInfo.callbackTrigger == CallbackTrigger.OnUpdate)
                        {
                            callbackInfo.Callback?.Invoke();
                        }
                    }
                }
            })
            .OnComplete(() =>
            {
                if (tweenInfo.tweenCallBacksContainer != null && tweenInfo.tweenCallBacksContainer.CallBacksInfo != null)
                {
                    foreach (var callbackInfo in tweenInfo.tweenCallBacksContainer.CallBacksInfo)
                    {
                        if (callbackInfo.callbackTrigger == CallbackTrigger.OnComplete)
                        {
                            callbackInfo.Callback?.Invoke();
                        }
                    }
                }
            });
        }

        private void HandleSequenceInsertion(TweenInfo tweenInfo, Tween tween, Sequence sequence)
        {
            if (tweenInfo.insertionType == TweenInsertionType.Insert)
            {
                sequence.Insert(tweenInfo.insertPostion, tween);
            }
            else if (tweenInfo.insertionType == TweenInsertionType.Append)
            {
                sequence.Append(tween);
            }
            else
            {
                sequence.Join(tween);
            }
        }

        private void HandleSequenceInsertionMultiple(Tween tween, Sequence sequence)
        {
            sequence.Join(tween);
        }

        private void DebugTarget()
        {
            Debug.LogWarning("Target in the TweenInfo is not assigned");
        }
    }



    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionFieldName { get; }
        public object[] ExpectedValues { get; }

        public ShowIfAttribute(string conditionFieldName, params object[] expectedValues)
        {
            ConditionFieldName = conditionFieldName;
            ExpectedValues = expectedValues;
        }
    }


#if UNITY_EDITOR

    [UnityEditor.CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;
            UnityEditor.SerializedProperty conditionProperty = GetConditionProperty(property, showIf.ConditionFieldName);

            if (conditionProperty != null && ShouldShowField(conditionProperty, showIf.ExpectedValues))
            {
                UnityEditor.EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;
            UnityEditor.SerializedProperty conditionProperty = GetConditionProperty(property, showIf.ConditionFieldName);

            if (conditionProperty != null && ShouldShowField(conditionProperty, showIf.ExpectedValues))
            {
                return UnityEditor.EditorGUI.GetPropertyHeight(property, label, true);
            }

            return 0f;
        }

        private UnityEditor.SerializedProperty GetConditionProperty(UnityEditor.SerializedProperty property, string conditionPath)
        {
            // Handles nested properties correctly
            string propertyPath = property.propertyPath; // e.g., "tweensInfo.Array.data[0].playFromStartValue"
            string parentPath = propertyPath.Substring(0, propertyPath.LastIndexOf('.')); // Extract parent path
            return property.serializedObject.FindProperty($"{parentPath}.{conditionPath}");
        }

        private bool ShouldShowField(UnityEditor.SerializedProperty conditionProperty, object[] expectedValues)
        {
            if (expectedValues == null || expectedValues.Length == 0) return true;

            switch (conditionProperty.propertyType)
            {
                case UnityEditor.SerializedPropertyType.Boolean:
                    return System.Array.Exists(expectedValues, value => (bool)value == conditionProperty.boolValue);

                case UnityEditor.SerializedPropertyType.Enum:
                    return System.Array.Exists(expectedValues, value => (int)value == conditionProperty.enumValueIndex);

                default:
                    Debug.LogWarning($"Unsupported property type: {conditionProperty.propertyType}");
                    return false;
            }
        }
    }


#endif

}
