using Isometric.Customer;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Environment
{
	public class TreadmillAnimationHandler : MonoBehaviour
	{
		[Header("---Station Info---")]
		[SerializeField]StationCustomerIn m_StationCustomerIn;
		[SerializeField] StationCustomerInHandler m_StationCustomerInHandler;

		[Header("---Station Service Info---")]
		[SerializeField] float m_BeforeCustomerSettleDelay = 0.2f;
		[SerializeField] float m_BeforeServiceStartDelay = 0.2f;
		[SerializeField] float m_BeforeCustomerLeaveDelay = 1.3f;
		[SerializeField] float m_CustomerSteadyWalkSpeedMultiplier = 3.25f;
		[SerializeField] SpriteRenderer m_TimerFillBarRenderer;

		[Tooltip("The name of a fill amount property inside the custom shader being utilized by the renderer's material.")]
        [SerializeField, ReadOnly] string m_FillAmountPropertyName = "_FillAmount";
		private Material m_TimerFillBarMaterial;
		private float m_TotalServiceDuration;

		[Header("---Animation Handlers---")]
		[SerializeField] TreadLineAnimationHandler m_TreadLineAnimationHandler;
		[SerializeField] StationCustomerInItemAnimationHandler m_MonitorAnimationHandler;
		[SerializeField] StationCustomerInItemAnimationHandler m_MonitorButtonsAnimationHandler;

		[Header("---Unity Events---")]
		[SerializeField] UnityEvent m_OnCustomerDraggedIn;
		[SerializeField] UnityEvent m_OnCustomerDraggedOut;
		[SerializeField] UnityEvent m_OnCustomerSettled;
		[SerializeField] UnityEvent m_OnServiceEnd;
		[SerializeField] UnityEvent m_OnCustomerLeft;


		private void OnEnable()
        {
            m_StationCustomerInHandler.OnMenuSetupStart += OnMenuSetupStart;
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
            m_StationCustomerInHandler.OnMenuSetupStart -= OnMenuSetupStart;
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

		private void OnMenuSetupStart()
		{
			SetTimerFill(0f);
		}

		private void OnGameplaySetupStart()
        {
            m_StationCustomerInHandler.SetServiceDelays(m_BeforeCustomerSettleDelay, m_BeforeServiceStartDelay, m_BeforeCustomerLeaveDelay);
        }

        private void OnCustomerDraggedIn()
        {
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.On, null, null);
            m_MonitorButtonsAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.StandBy, null, null);
            m_OnCustomerDraggedIn?.Invoke();
        }
        private void OnCustomerDraggedOut()
        {
			m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Off, null, null);
            m_MonitorButtonsAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, null, null);
            m_OnCustomerDraggedOut?.Invoke();
        }
        private void OnCustomerDropped(CustomerSalonController customerSalonController)
		{
			customerSalonController.SetSteadyWalkSpeedMultiplier(m_CustomerSteadyWalkSpeedMultiplier);
		}
        private void OnCustomerSettled()
        {
            m_MonitorButtonsAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Enable, null, null);
			m_TreadLineAnimationHandler.StartMovement();
            m_OnCustomerSettled?.Invoke();
        }
        private void OnServiceStart(float totalDuration)
        {
            m_TotalServiceDuration = totalDuration;
            SetTimerFill(0f);
			m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.DisplayWave, null, null);
        }
        private void OnServiceTimerUpdate(float timeElapsed)
        {
            float fillAmount = Mathf.Clamp01(timeElapsed / m_TotalServiceDuration);
            SetTimerFill(fillAmount);
        }
        private void OnServiceEnd()
        {
            SetTimerFill(1f);
			m_TreadLineAnimationHandler.StopMovement();
			m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.On, null, null);
            m_MonitorButtonsAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.StandBy, null, null);
            m_OnServiceEnd?.Invoke();
        }
        private void OnCustomerLeft()
        {
            SetTimerFill(0f);
            m_MonitorAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Off, null, null);
            m_MonitorButtonsAnimationHandler.PlayState(StationCustomerInItemAnimatorStates.Disable, null, null);
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
	}
}
