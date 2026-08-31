using NaughtyAttributes;
using UnityEngine;

namespace Isometric.Environment
{
	public class TreadLineAnimationHandler : MonoBehaviour
	{
		[Header("---Holders---")]
		[InfoBox("Holder 1 must be placed at the Middle Position initially, i.e. on the treadmill!", EInfoBoxType.Normal)]
		[SerializeField] Transform m_Holder1;

		[Space]
		[InfoBox("Holder 2 must be placed at the Back Position initially, i.e. behind the treadmill!", EInfoBoxType.Normal)]
		[SerializeField] Transform m_Holder2;

		[Header("---Movement---")]
		[Tooltip("Time required to move one holder-spacing.")]
		[Min(0.01f), SerializeField] float m_MoveDuration = 1.7f;

		[Tooltip("How softly the tread reaches its full speed.")]
		[Min(0.01f), SerializeField] float m_SmoothStartDuration = 0.35f;

		[Tooltip("How softly the tread slows down when stopped.")]
		[Min(0.01f), SerializeField] float m_SmoothStopDuration = 0.2f;

		private Transform m_FrontHolder;
		private Transform m_BackHolder;
		private Vector3 m_BackPosition;
		private Vector3 m_MiddlePosition;
		private Vector3 m_OutsidePosition;

		private bool m_IsMoving;
		private float m_MovementProgress;

		private float m_CurrentSpeedMultiplier;
		private float m_SmoothStartVelocity;
		private float m_SmoothStopVelocity;


		private void Awake()
		{
			m_MiddlePosition = m_Holder1.localPosition;
			m_BackPosition = m_Holder2.localPosition;
			Vector3 holderSpacing = m_MiddlePosition - m_BackPosition;
			m_OutsidePosition = m_MiddlePosition + holderSpacing;

			m_FrontHolder = m_Holder1;
			m_BackHolder = m_Holder2;

			m_MovementProgress = 0f;
			m_CurrentSpeedMultiplier = 0f;
		}

		private void Update()
		{
			if (m_IsMoving)
			{
				m_CurrentSpeedMultiplier = Mathf.SmoothDamp(m_CurrentSpeedMultiplier, 1f, ref m_SmoothStartVelocity, m_SmoothStartDuration);
				
				if (m_CurrentSpeedMultiplier > 0.999f)
				{
					m_CurrentSpeedMultiplier = 1f;
					m_SmoothStartVelocity = 0f;
				}
			}
			else
			{
				m_CurrentSpeedMultiplier = Mathf.SmoothDamp(m_CurrentSpeedMultiplier, 0f, ref m_SmoothStopVelocity, m_SmoothStopDuration);

				if (m_CurrentSpeedMultiplier < 0.001f)
				{
					m_CurrentSpeedMultiplier = 0f;
					m_SmoothStopVelocity = 0f;
				}
			}

			if (m_CurrentSpeedMultiplier <= 0f)
				return;
				
			float moveDuration = Mathf.Max(0.01f, m_MoveDuration);
			m_MovementProgress += (Time.deltaTime / moveDuration) * m_CurrentSpeedMultiplier;

			// When one holder reaches the outside, swap the holder roles and continue seamlessly.
			while (m_MovementProgress >= 1f)
			{
				m_MovementProgress -= 1f;

				Transform previousFrontHolder = m_FrontHolder;
				m_FrontHolder = m_BackHolder;
				m_BackHolder = previousFrontHolder;
			}

			// Move the front holder:
			// Middle -> Outside
			m_FrontHolder.localPosition = Vector3.LerpUnclamped(m_MiddlePosition, m_OutsidePosition, m_MovementProgress);

			// Move the back holder:
			// Back -> Middle
			m_BackHolder.localPosition = Vector3.LerpUnclamped(m_BackPosition, m_MiddlePosition, m_MovementProgress);
		}

		[ContextMenu("StartMovement")]
		public void StartMovement()
		{
			m_IsMoving = true;
			m_SmoothStopVelocity = 0f;
		}

		[ContextMenu("StopMovement")]
		public void StopMovement()
		{
			m_IsMoving = false;
			m_SmoothStartVelocity = 0f;
		}
	}
}