using DG.Tweening;
using Isometric.Customer;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Environment
{
    public class XRayAnimationHandler : MonoBehaviour
    {
        [Header("---Station Info---")]
        [SerializeField] StationCustomerIn m_StationCustomerIn;
        [SerializeField] StationCustomerInHandler m_StationCustomerInHandler;

        [Header("---Station Service Info---")]
        [SerializeField] float m_BeforeCustomerSettleDelay = 0f;
        [SerializeField] float m_BeforeServiceStartDelay = 0f; 
        [SerializeField] float m_BeforeCustomerLeaveDelay = 0f;
        [SerializeField] SpriteRenderer m_TimerFillBarRenderer;
        [Tooltip("The name of a fill amount property inside the custom shader being utilized by the renderer's material.")]
        [SerializeField, ReadOnly] string m_FillAmountPropertyName = "_FillAmount";
        private Material m_TimerFillBarMaterial = null;
        private float m_TotalServiceDuration;

        [Header("---X-Ray Scanning Tween Info---")]
        [SerializeField] Transform m_XRayHolder;
        [SerializeField] Vector3 m_XRayScanTopLocalPos;
        [SerializeField] Vector3 m_XRayScanBottomLocalPos;
        [SerializeField] Ease m_XRayScanEaseType = Ease.Linear;
        [SerializeField] bool m_UseXRayHalfScanDelay = true;
        [ShowIf(nameof(m_UseXRayHalfScanDelay))]
        [SerializeField] float m_XRayDelayAfterHalfScan = 0.4f;

        [Header("---Monitor Scanning Tween Info---")]
        [SerializeField] Transform m_MonitorScannerHolder;
        [SerializeField] Vector3 m_MonitorScanTopLocalPos;
        [SerializeField] Vector3 m_MonitorScanBottomLocalPos;
        [SerializeField] Ease m_MonitorScanEaseType = Ease.Linear;

        [Header("---Animation Handlers---")]
        [SerializeField] StationCustomerInItemAnimationHandler m_GlassDoorAnimationHandler;
        [SerializeField] StationCustomerInItemAnimationHandler m_MonitorAnimationHandler;
        [SerializeField] StationCustomerInItemAnimationHandler m_MonitorSkeletonAnimationHandler;
        [SerializeField] StationCustomerInItemAnimationHandler m_MonitorScannerAnimationHandler;
        [SerializeField] StationCustomerInItemAnimationHandler m_TimerBarAnimationHandler;
        [SerializeField] StationCustomerInItemAnimationHandler m_XRayAnimationHandler;

        [Header("---Unity Events---")]
        [SerializeField] UnityEvent m_OnCustomerDraggedIn;
        [SerializeField] UnityEvent m_OnCustomerDraggedOut;
        [SerializeField] UnityEvent m_OnCustomerSettled;
        [SerializeField] UnityEvent m_OnServiceEnd;
        [SerializeField] UnityEvent m_OnCustomerLeft;

        private void OnEnable()
        {
            m_StationCustomerInHandler.OnGameplaySetupStart += OnGameplaySetupStart;
            m_StationCustomerInHandler.OnCustomerDraggedIn += OnCustomerDraggedIn;
            m_StationCustomerInHandler.OnCustomerDraggedOut += OnCustomerDraggedOut;
            m_StationCustomerInHandler.OnCustomerDropped += OnCustomerDropped;
            m_StationCustomerInHandler.OnCustomerSettled += OnCustomerSettled;
            m_StationCustomerInHandler.OnServiceStart += OnServiceStart;
            m_StationCustomerInHandler.OnServiceTimerUpdate += OnServiceTimerUpdate;
            m_StationCustomerInHandler.OnServiceEnd += OnServiceEnd;
            m_StationCustomerInHandler.OnCustomerLeft += OnCustomerLeft;
            m_StationCustomerInHandler.OnInstantOrderComplete += OnInstantOrderComplete;
        }
        private void OnDisable()
        {
            m_StationCustomerInHandler.OnGameplaySetupStart -= OnGameplaySetupStart;  
            m_StationCustomerInHandler.OnCustomerDraggedIn -= OnCustomerDraggedIn;
            m_StationCustomerInHandler.OnCustomerDraggedOut -= OnCustomerDraggedOut;
            m_StationCustomerInHandler.OnCustomerDropped -= OnCustomerDropped;
            m_StationCustomerInHandler.OnCustomerSettled -= OnCustomerSettled;
            m_StationCustomerInHandler.OnServiceStart -= OnServiceStart;
            m_StationCustomerInHandler.OnServiceTimerUpdate -= OnServiceTimerUpdate;
            m_StationCustomerInHandler.OnServiceEnd -= OnServiceEnd;
            m_StationCustomerInHandler.OnCustomerLeft -= OnCustomerLeft;
            m_StationCustomerInHandler.OnInstantOrderComplete -= OnInstantOrderComplete;
        }

        private void OnGameplaySetupStart()
        {
            m_StationCustomerInHandler.SetServiceDelays(m_BeforeCustomerSettleDelay, m_BeforeServiceStartDelay, m_BeforeCustomerLeaveDelay);
        }

        private void OnCustomerDraggedIn()
        {
            // m_GlassDoorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Open, null, null);
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.On, false, null, null);
            m_OnCustomerDraggedIn?.Invoke();
        }
        private void OnCustomerDraggedOut()
        {
            // m_GlassDoorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Close, null, null);
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Off, false, null, null);
            m_OnCustomerDraggedOut?.Invoke();
        }
        private void OnCustomerDropped(CustomerSalonController customerSalonController)
        {
            m_GlassDoorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Close, false, null, null);
        }
        private void OnCustomerSettled()
        {
            m_XRayAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Enable, false, () =>
            {
                m_XRayHolder.localPosition = m_XRayScanTopLocalPos;
            }, null);
            m_MonitorScannerAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Enable, false, () =>
            {
                m_MonitorScannerHolder.localPosition = m_MonitorScanTopLocalPos;
                m_MonitorSkeletonAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Enable, false, null, null);
            }, null);
            m_OnCustomerSettled?.Invoke();
        }
        private void OnServiceStart(float totalDuration)
        {
            m_TotalServiceDuration = totalDuration;
            SetTimerFill(0f);
            m_TimerBarAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Enable, false, null, null);
            StartXRayScan();
            StartMonitorScan();
        }
        private void OnServiceTimerUpdate(float timeElapsed)
        {
            float fillAmount = Mathf.Clamp01(timeElapsed / m_TotalServiceDuration);
            SetTimerFill(fillAmount);
        }
        private void OnServiceEnd()
        {
            SetTimerFill(1f);
            m_XRayAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, false, null, null);
            m_MonitorScannerAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, false, null, null);
            m_TimerBarAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, false, null, () =>
            {
                SetTimerFill(0f);
            });
            m_MonitorSkeletonAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.ScalePingPong, false, null, null);
            m_GlassDoorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Open, false, null, null);
            m_OnServiceEnd?.Invoke();
        }
        private void OnCustomerLeft()
        {
            // m_GlassDoorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Close, null, null);
            m_MonitorSkeletonAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, false, null, null);
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Off, false, null, null);
            m_OnCustomerLeft?.Invoke();
        }

        private void OnInstantOrderComplete()
        {
            m_XRayHolder.DOKill();
            m_MonitorScannerHolder.DOKill();
            m_GlassDoorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Open, true, null, null);
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Off, true, null, null);
            m_MonitorSkeletonAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, true, null, null);
            m_MonitorScannerAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, true, null, null);
            m_TimerBarAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, true, null, null);
            m_XRayAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, true, null, null);
            SetTimerFill(0f);
            m_OnCustomerLeft?.Invoke();
        }

        private void SetTimerFill(float fillAmount)
        {
            if(m_TimerFillBarMaterial == null)
            {
                m_TimerFillBarMaterial = m_TimerFillBarRenderer.material;
            }

            m_TimerFillBarMaterial.SetFloat(m_FillAmountPropertyName, fillAmount);
        }
        
        private void StartXRayScan()
        {
            float duration = m_UseXRayHalfScanDelay ? (m_TotalServiceDuration - m_XRayDelayAfterHalfScan) / 2f : 
            m_TotalServiceDuration / 2f;
            m_XRayHolder.DOLocalMove(m_XRayScanBottomLocalPos, duration).SetEase(m_XRayScanEaseType).OnComplete(() =>
            {
                float delay = m_UseXRayHalfScanDelay ? m_XRayDelayAfterHalfScan : 0f;
                m_XRayHolder.DOLocalMove(m_XRayScanTopLocalPos, duration).SetEase(m_XRayScanEaseType).SetDelay(delay);
            });
        }
        private void StartMonitorScan()
        {
            float duration = m_TotalServiceDuration;
            m_MonitorScannerHolder.DOLocalMove(m_MonitorScanBottomLocalPos, duration).SetEase(m_MonitorScanEaseType);
        }
    }
}