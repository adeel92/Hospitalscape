using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Environment
{
    public class MRIAnimationHandler : MonoBehaviour
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

        [Header("---Animation Handlers---")]
        [SerializeField] StationCustomerInItemAnimationHandler m_BedAnimationHandler;
        [SerializeField] StationCustomerInItemAnimationHandler m_MonitorAnimationHandler;
        [SerializeField] StationCustomerInItemAnimationHandler m_RadiationEmitterAnimationHandler;
        [SerializeField] StationCustomerInItemAnimationHandler m_TimerBarAnimationHandler;

        [Header("---Unity Events---")]
        [SerializeField] UnityEvent m_OnCustomerDraggedIn;
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
        }

        private void OnGameplaySetupStart()
        {
            m_StationCustomerInHandler.SetServiceDelays(m_BeforeCustomerSettleDelay, m_BeforeServiceStartDelay, m_BeforeCustomerLeaveDelay);
        }

        private void OnCustomerDraggedIn()
        {
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Enable, null, null);
            m_OnCustomerDraggedIn?.Invoke();
        }
        private void OnCustomerDraggedOut()
        {
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, null, null);
        }
        private void OnCustomerDropped()
        {
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Hold, null, null);
            m_TimerBarAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Hold, null, null);
        }
        private void OnCustomerSettled()
        {
            m_BedAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.MoveInside, null, null);
        }
        private void OnServiceStart(float totalDuration)
        {
            m_TotalServiceDuration = totalDuration;
            SetTimerFill(0f);
            m_RadiationEmitterAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Enable, null, null);
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.DisplayWave, null, null);
            m_TimerBarAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Enable, null, null);
        }
        private void OnServiceTimerUpdate(float timeElapsed)
        {
            float fillAmount = Mathf.Clamp01(timeElapsed / m_TotalServiceDuration);
            SetTimerFill(fillAmount);
        }
        private void OnServiceEnd()
        {
            SetTimerFill(1f);
            m_RadiationEmitterAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, null, null);
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.ScanComplete, null, null);
            m_TimerBarAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.ScanComplete, null, null);
            m_BedAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.MoveOutside, null, null);
        }
        private void OnCustomerLeft()
        {
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, null, null);
            m_TimerBarAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, null, () =>
            {
                SetTimerFill(0f);
            });
            m_OnCustomerLeft?.Invoke();
        }

        public void SetTimerFill(float fillAmount)
        {
            if(m_TimerFillBarMaterial == null)
            {
                m_TimerFillBarMaterial = m_TimerFillBarRenderer.material;
            }

            m_TimerFillBarMaterial.SetFloat(m_FillAmountPropertyName, fillAmount);
        }
    }
}