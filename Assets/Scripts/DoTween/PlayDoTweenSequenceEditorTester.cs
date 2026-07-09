using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

namespace Arc
{
    public class PlayDoTweenSequenceEditorTester : MonoBehaviour
    {
        public enum CallType { Manual, OnStart, OnEnable}
        public enum TweenInsertionType { Insert, Append, Join}
        public enum TweenType { Move, Rotate, Scale, AnchorMove, Fade, Callback, Interval, AnchorSizeDelta}
        public enum CallbackTrigger { OnStart, OnUpdate, OnComplete}

        [Serializable]
        public class TweenInfo
        {
            public TweenInsertionType insertionType;
            [TweenTesterShowIf(nameof(insertionType), TweenInsertionType.Insert)]
            public float insertPostion;

            [Space]
            public TweenType tweenType;

            [TweenTesterShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.Fade)]
            public Transform targetTransform;

            [TweenTesterShowIf(nameof(tweenType), TweenType.AnchorMove,  TweenType.AnchorSizeDelta)]
            public RectTransform targetRectTransform;

            [TweenTesterShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove, TweenType.Fade, TweenType.AnchorSizeDelta)]
            public Ease easeType;
            [TweenTesterShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove, TweenType.Fade, TweenType.AnchorSizeDelta)]
            public float duration;
            [TweenTesterShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove, TweenType.Fade,  TweenType.AnchorSizeDelta)]
            public float delay;

            [TweenTesterShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove,  TweenType.AnchorSizeDelta)]
            public bool useStartTransformTweenValue = true;
            [TweenTesterShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove,  TweenType.AnchorSizeDelta)]
            public Vector3 startTransformTweenValue;
            [TweenTesterShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove,  TweenType.AnchorSizeDelta)]
            public Vector3 endTransformTweenValue;
            [TweenTesterShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate)]
            public bool isLocal = false;

            [TweenTesterShowIf(nameof(tweenType), TweenType.Fade)]
            public bool useStartFadeTweenValue = true;
            [TweenTesterShowIf(nameof(tweenType), TweenType.Fade), Range(0, 1)]
            public float startFadeTweenValue = 0;
            [TweenTesterShowIf(nameof(tweenType), TweenType.Fade), Range(0, 1)]
            public float endFadeTweenValue = 1;
            [TweenTesterShowIf(nameof(tweenType), TweenType.Fade)]
            public bool useForAllChilds = false;

            [TweenTesterShowIf(nameof(tweenType), TweenType.Move, TweenType.Rotate, TweenType.Scale, TweenType.AnchorMove, TweenType.Fade,  TweenType.AnchorSizeDelta)]
            public CallBacksContainer tweenCallBacksContainer;

            [TweenTesterShowIf(nameof(tweenType), TweenType.Callback)]
            public UnityEvent callback;

            [TweenTesterShowIf(nameof(tweenType), TweenType.Interval)]
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

#if UNITY_EDITOR
        [Header("Editor Preview Only \n - Used only when previewing from the \n Inspector in Edit Mode.")]
        [Tooltip("Edit Mode only. Invokes UnityEvents during preview. Keep off unless you need to test event callbacks like enabling objects, particles, or sounds.")]
        [Space] public bool editorPreviewInvokeCallbacks;
        [Tooltip("Edit Mode only. Controls the preview playback speed. 1 = normal speed, 0.5 = slower, 2 = faster. Runtime speed is not affected.")]
        [Min(0.01f)] public float editorPreviewSpeed = 1f;
        [Tooltip("Edit Mode only. Repeats the preview sequence continuously in the editor. Runtime looping is not affected.")]
        public bool editorPreviewLoop = false;
#endif

        Sequence tweenSequence = null;

#if UNITY_EDITOR
        private bool editorPreviewIsRunning;
        private double editorPreviewLastEditorTime;
        private float editorPreviewElapsed;
        private float editorPreviewDuration;

        public bool IsEditorPreviewRunning => editorPreviewIsRunning;
#endif

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
            return PlaySequenceInternal(onComplete, true);
        }

        private Sequence PlaySequenceInternal(Action onComplete, bool invokeCallbacks)
        {
            Stop();

            tweenSequence = DOTween.Sequence();


            foreach (var tweenInfo in tweensInfo)
            {
                if (tweenInfo.tweenType == TweenType.Move)
                {
                    HandleMoveTweenInfo(tweenInfo, tweenSequence, invokeCallbacks);
                }
                else if (tweenInfo.tweenType == TweenType.Rotate)
                {
                    HandleRotateTweenInfo(tweenInfo, tweenSequence, invokeCallbacks);
                }
                else if (tweenInfo.tweenType == TweenType.Scale)
                {
                    HandleScaleTweenInfo(tweenInfo, tweenSequence, invokeCallbacks);
                }
                else if (tweenInfo.tweenType == TweenType.AnchorMove)
                {
                    HandleAnchorMoveTweenInfo(tweenInfo, tweenSequence, invokeCallbacks);
                }
                else if (tweenInfo.tweenType == TweenType.AnchorSizeDelta)
                {
                    HandleAnchorSizeDeltaTweenInfo(tweenInfo, tweenSequence, invokeCallbacks);
                }
                else if (tweenInfo.tweenType == TweenType.Fade)
                {
                    HandleFadeTweenInfo(tweenInfo, tweenSequence, invokeCallbacks);
                }
                else if (tweenInfo.tweenType == TweenType.Callback)
                {
                    HandleCallbackTweenInfo(tweenInfo, tweenSequence, invokeCallbacks);
                }
                else if (tweenInfo.tweenType == TweenType.Interval)
                {
                    HandleIntervalTweenInfo(tweenInfo, tweenSequence);
                }
            }

            tweenSequence.SetUpdate(ignoreTimeScale)
            .OnStart(() =>
            {
                if (invokeCallbacks && SequenceCallBacksInfo != null && SequenceCallBacksInfo.CallBacksInfo != null)
                {
                    foreach (var callback in SequenceCallBacksInfo.CallBacksInfo) { if (callback.callbackTrigger == CallbackTrigger.OnStart) { callback.Callback?.Invoke(); } }
                }
            })
            .OnUpdate(() =>
            {
                if (invokeCallbacks && SequenceCallBacksInfo != null && SequenceCallBacksInfo.CallBacksInfo != null)
                {
                    foreach (var callback in SequenceCallBacksInfo.CallBacksInfo) { if (callback.callbackTrigger == CallbackTrigger.OnUpdate) { callback.Callback?.Invoke(); } }
                }
            })
            .OnComplete(() =>
            {
                if (invokeCallbacks && SequenceCallBacksInfo != null && SequenceCallBacksInfo.CallBacksInfo != null)
                {
                    foreach (var callback in SequenceCallBacksInfo.CallBacksInfo) { if (callback.callbackTrigger == CallbackTrigger.OnComplete) { callback.Callback?.Invoke(); } }
                }

                onComplete?.Invoke();
            });

            return tweenSequence;
        }

        public void Stop()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                StopEditorPreview(false, true);
                return;
            }
#endif
            KillSequence();
        }

        private void KillSequence()
        {
            if (tweenSequence != null)
            {
                tweenSequence.Kill();
                tweenSequence = null;
            }
        }

        private void HandleMoveTweenInfo(TweenInfo tweenInfo, Sequence sequence, bool invokeCallbacks)
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
                HandleTweenCallback(tweenInfo, movetween, invokeCallbacks);
                HandleSequenceInsertion(tweenInfo, movetween, sequence);
            }
            else
            {
                //InsertCallback(tweenInfo, () => { if (tweenInfo.useStartTransformTweenValue) target.localPosition = tweenInfo.startTransformTweenValue; }, sequence);
                if (tweenInfo.useStartTransformTweenValue) target.localPosition = tweenInfo.startTransformTweenValue;
                Tween movetween = target.DOLocalMove(tweenInfo.endTransformTweenValue, tweenInfo.duration)
                    .SetEase(tweenInfo.easeType)
                    .SetDelay(tweenInfo.delay);
                HandleTweenCallback(tweenInfo, movetween, invokeCallbacks);
                HandleSequenceInsertion(tweenInfo, movetween, sequence);
            }
        }

        private void HandleRotateTweenInfo(TweenInfo tweenInfo, Sequence sequence, bool invokeCallbacks)
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
                HandleTweenCallback(tweenInfo, movetween, invokeCallbacks);
                HandleSequenceInsertion(tweenInfo, movetween, sequence);
            }
            else
            {
                //InsertCallback(tweenInfo, () => { if (tweenInfo.useStartTransformTweenValue) target.localEulerAngles = tweenInfo.startTransformTweenValue; }, sequence);
                if (tweenInfo.useStartTransformTweenValue) target.localEulerAngles = tweenInfo.startTransformTweenValue;
                Tween movetween = target.DOLocalRotate(tweenInfo.endTransformTweenValue, tweenInfo.duration)
                    .SetEase(tweenInfo.easeType)
                    .SetDelay(tweenInfo.delay);
                HandleTweenCallback(tweenInfo, movetween, invokeCallbacks);
                HandleSequenceInsertion(tweenInfo, movetween, sequence);
            }
        }

        private void HandleScaleTweenInfo(TweenInfo tweenInfo, Sequence sequence, bool invokeCallbacks)
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
            HandleTweenCallback(tweenInfo, movetween, invokeCallbacks);
            HandleSequenceInsertion(tweenInfo, movetween, sequence);
        }

        private void HandleAnchorMoveTweenInfo(TweenInfo tweenInfo, Sequence sequence, bool invokeCallbacks)
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
            HandleTweenCallback(tweenInfo, movetween, invokeCallbacks);
            HandleSequenceInsertion(tweenInfo, movetween, sequence);
        }

         private void HandleAnchorSizeDeltaTweenInfo(TweenInfo tweenInfo, Sequence sequence, bool invokeCallbacks)
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
            HandleTweenCallback(tweenInfo, movetween, invokeCallbacks);
            HandleSequenceInsertion(tweenInfo, movetween, sequence);
        }

        private void HandleFadeTweenInfo(TweenInfo tweenInfo, Sequence sequence, bool invokeCallbacks)
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
                    HandleTweenCallback(tweenInfo, tweenFade, invokeCallbacks);
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
                        HandleTweenCallback(tweenInfo, tweenFade, invokeCallbacks);
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
                    HandleTweenCallback(tweenInfo, tweenFade, invokeCallbacks);
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
                    HandleTweenCallback(tweenInfo, tweenFade, invokeCallbacks);
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
                    HandleTweenCallback(tweenInfo, tweenFade, invokeCallbacks);
                    HandleSequenceInsertion(tweenInfo, tweenFade, sequence);
                }
            }
        }


        private void HandleCallbackTweenInfo(TweenInfo tweenInfo, Sequence sequence, bool invokeCallbacks)
        {
            if (!invokeCallbacks)
            {
                return;
            }

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

        private void HandleTweenCallback(TweenInfo tweenInfo, Tween tween, bool invokeCallbacks)
        {
            if (!invokeCallbacks)
            {
                return;
            }

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

#if UNITY_EDITOR
        public void EditorPreviewPlay()
        {
            if (Application.isPlaying)
            {
                PlaySequence();
                return;
            }

            StopEditorPreview(false, true);

            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);

            tweenSequence = PlaySequenceInternal(null, editorPreviewInvokeCallbacks);
            if (tweenSequence == null)
            {
                return;
            }

            tweenSequence.SetAutoKill(false);
            tweenSequence.Pause();
            tweenSequence.ForceInit();

            editorPreviewDuration = tweenSequence.Duration(false);
            editorPreviewElapsed = 0f;
            editorPreviewLastEditorTime = UnityEditor.EditorApplication.timeSinceStartup;
            editorPreviewIsRunning = true;

            tweenSequence.Goto(0f, false);

            UnityEditor.EditorApplication.update -= EditorPreviewUpdate;
            UnityEditor.EditorApplication.update += EditorPreviewUpdate;
            RepaintEditorPreviewViews();
        }

        public void EditorPreviewStop()
        {
            if (Application.isPlaying)
            {
                Stop();
                return;
            }

            StopEditorPreview(false, true);
        }

        public void EditorPreviewRewindToStart()
        {
            if (Application.isPlaying)
            {
                Stop();
                return;
            }

            if (tweenSequence == null || !tweenSequence.IsActive())
            {
                DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
                tweenSequence = PlaySequenceInternal(null, false);
                if (tweenSequence != null)
                {
                    tweenSequence.SetAutoKill(false);
                    tweenSequence.Pause();
                    tweenSequence.ForceInit();
                }
            }

            StopEditorPreview(true, true);
        }

        public void EditorPreviewCompleteToEnd()
        {
            if (Application.isPlaying)
            {
                if (tweenSequence != null && tweenSequence.IsActive())
                {
                    tweenSequence.Complete();
                }
                return;
            }

            if (tweenSequence == null || !tweenSequence.IsActive())
            {
                DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
                tweenSequence = PlaySequenceInternal(null, editorPreviewInvokeCallbacks);
                if (tweenSequence == null)
                {
                    return;
                }

                tweenSequence.SetAutoKill(false);
                tweenSequence.Pause();
                tweenSequence.ForceInit();
            }

            tweenSequence.Goto(tweenSequence.Duration(false), false);
            StopEditorPreview(false, true);
        }

        private void EditorPreviewUpdate()
        {
            if (Application.isPlaying)
            {
                StopEditorPreview(false, true);
                return;
            }

            if (!editorPreviewIsRunning || tweenSequence == null || !tweenSequence.IsActive())
            {
                StopEditorPreview(false, true);
                return;
            }

            double currentEditorTime = UnityEditor.EditorApplication.timeSinceStartup;
            float deltaTime = Mathf.Max(0f, (float)(currentEditorTime - editorPreviewLastEditorTime));
            editorPreviewLastEditorTime = currentEditorTime;

            editorPreviewElapsed += deltaTime * Mathf.Max(0.01f, editorPreviewSpeed);

            if (editorPreviewDuration <= 0f)
            {
                tweenSequence.Goto(0f, false);
                StopEditorPreview(false, false);
                return;
            }

            if (editorPreviewElapsed >= editorPreviewDuration)
            {
                tweenSequence.Goto(editorPreviewDuration, false);

                if (editorPreviewLoop)
                {
                    editorPreviewElapsed = 0f;
                    editorPreviewLastEditorTime = currentEditorTime;
                    tweenSequence.Goto(0f, false);
                }
                else
                {
                    StopEditorPreview(false, false);
                }
            }
            else
            {
                tweenSequence.Goto(editorPreviewElapsed, false);
            }

            RepaintEditorPreviewViews();
        }

        private void StopEditorPreview(bool resetToStart, bool killSequence)
        {
            UnityEditor.EditorApplication.update -= EditorPreviewUpdate;
            editorPreviewIsRunning = false;

            if (tweenSequence != null && tweenSequence.IsActive())
            {
                if (resetToStart)
                {
                    tweenSequence.Goto(0f, false);
                }

                if (killSequence)
                {
                    KillSequence();
                }
            }

            RepaintEditorPreviewViews();
        }

        private void RepaintEditorPreviewViews()
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
        }
#endif

        private void DebugTarget()
        {
            Debug.LogWarning("Target in the TweenInfo is not assigned");
        }
    }



    public class TweenTesterShowIfAttribute : PropertyAttribute
    {
        public string ConditionFieldName { get; }
        public object[] ExpectedValues { get; }

        public TweenTesterShowIfAttribute(string conditionFieldName, params object[] expectedValues)
        {
            ConditionFieldName = conditionFieldName;
            ExpectedValues = expectedValues;
        }
    }


#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(PlayDoTweenSequenceEditorTester))]
    public class PlayDoTweenSequenceEditorTesterInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UnityEditor.EditorGUILayout.Space(10f);
            UnityEditor.EditorGUILayout.LabelField("Editor Sequence Preview", UnityEditor.EditorStyles.boldLabel);

            PlayDoTweenSequenceEditorTester sequencePlayer = (PlayDoTweenSequenceEditorTester)target;

            using (new UnityEditor.EditorGUILayout.HorizontalScope())
            {
                if (UnityEngine.GUILayout.Button(sequencePlayer.IsEditorPreviewRunning ? "Restart Preview" : "Play Preview"))
                {
                    sequencePlayer.EditorPreviewPlay();
                }

                if (UnityEngine.GUILayout.Button("Stop"))
                {
                    sequencePlayer.EditorPreviewStop();
                }
            }

            using (new UnityEditor.EditorGUILayout.HorizontalScope())
            {
                if (UnityEngine.GUILayout.Button("Rewind To Start"))
                {
                    sequencePlayer.EditorPreviewRewindToStart();
                }

                if (UnityEngine.GUILayout.Button("Complete To End"))
                {
                    sequencePlayer.EditorPreviewCompleteToEnd();
                }
            }

            if (!UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorGUILayout.HelpBox("These buttons preview the same tween list in Edit Mode, so you can test popup animation without entering Play Mode. Keep callbacks disabled unless you specifically want UnityEvents to run in the editor.", UnityEditor.MessageType.Info);
            }
        }
    }

    [UnityEditor.CustomPropertyDrawer(typeof(TweenTesterShowIfAttribute))]
    public class TweenTesterShowIfDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            TweenTesterShowIfAttribute showIf = (TweenTesterShowIfAttribute)attribute;
            UnityEditor.SerializedProperty conditionProperty = GetConditionProperty(property, showIf.ConditionFieldName);

            if (conditionProperty != null && ShouldShowField(conditionProperty, showIf.ExpectedValues))
            {
                UnityEditor.EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
        {
            TweenTesterShowIfAttribute showIf = (TweenTesterShowIfAttribute)attribute;
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
