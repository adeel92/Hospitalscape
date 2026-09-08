using System;
using System.Collections.Generic;
using DG.Tweening;
using Isometric.Data;
using NaughtyAttributes;
using UnityEngine;

namespace Isometric.Environment
{
    public class ReportMachineAnimationHandler : MonoBehaviour
    {
        [Header("---Station Info---")]
        [SerializeField] StationEngageOrderOutOnCustomerDemand m_StationEngageOrderOutOnCustomerDemand;

        [Header("---Station Service Info---")]
        [SerializeField] float m_DelayBeforeDurationStart = 0.25f;
        [SerializeField] float m_DelayBeforeOrderOut = 0.3f;
        [SerializeField] SpriteRenderer m_TimerFillBarRenderer;
        [Tooltip("The name of a fill amount property inside the custom shader being utilized by the renderer's material.")]
        [SerializeField, ReadOnly] string m_FillAmountPropertyName = "_FillAmount";
        private Material m_TimerFillBarMaterial = null;
        [Space, SerializeField] List<ReportInfo> m_ReportInfos = new();

        [Header("---Animation Handlers---")]
        [SerializeField] StationEngageOrderOutItemAnimationHandler m_MachineAnimationHandler;

        [Space]
        [SerializeField, ReadOnly] bool m_IsHandlingOrder;


        private void Awake()
        {
            m_StationEngageOrderOutOnCustomerDemand.SetTaskProcessDelays(m_DelayBeforeDurationStart, m_DelayBeforeOrderOut);
        }
        private void OnEnable()
        {
            m_StationEngageOrderOutOnCustomerDemand.OnDemandedCustomerAdded += HandleOrder;
            m_StationEngageOrderOutOnCustomerDemand.OnProcessStart += OnProcessStart;
            m_StationEngageOrderOutOnCustomerDemand.OnProcessComplete += OnProcessComplete;
            m_StationEngageOrderOutOnCustomerDemand.OnInstantProcessComplete += OnInstantProcessComplete;
        }
        private void OnDisable()
        {
            m_StationEngageOrderOutOnCustomerDemand.OnDemandedCustomerAdded -= HandleOrder;
            m_StationEngageOrderOutOnCustomerDemand.OnProcessStart -= OnProcessStart;
            m_StationEngageOrderOutOnCustomerDemand.OnProcessComplete -= OnProcessComplete;
            m_StationEngageOrderOutOnCustomerDemand.OnInstantProcessComplete = OnInstantProcessComplete;
        }

        private void UpdateDemandedReportVisuals(CustomerFirstOrderInfo firstOrder)
        {
            foreach(ReportInfo reportInfo in m_ReportInfos)
            {
                if(reportInfo.FirstOrder.OrderConsumable == firstOrder.OrderConsumable)
                {
                    reportInfo.TargetScreenReport.SetActive(true);
                    reportInfo.TargetMagnifyingGlass.SetActive(true);
                    reportInfo.TargetOutputReport.SetActive(true);
                    m_TimerFillBarRenderer.color = reportInfo.TargetFillBarColor;
                }
                else
                {
                    reportInfo.TargetScreenReport.SetActive(false);
                    reportInfo.TargetMagnifyingGlass.SetActive(false);
                    reportInfo.TargetOutputReport.SetActive(false);
                }
            }
        }

        private void HandleOrder()
        {
            if(m_IsHandlingOrder)
                return;

            StationEngageOrderOutOnCustomerDemand.DemandedCustomerInfo firstDemandedCustomerInfo = m_StationEngageOrderOutOnCustomerDemand.GetFirstDemandedCustomer();
            if(firstDemandedCustomerInfo != null)
            {
                m_IsHandlingOrder = true;
                ResetTimer();
                UpdateDemandedReportVisuals(firstDemandedCustomerInfo.CustomerFirstOrderInfo);
                m_MachineAnimationHandler.PlayState(StationEngageOrderOutItemAnimatorStates.On, false, null, null);
            }
        }
        private void OnProcessStart()
        {
            m_MachineAnimationHandler.PlayState(StationEngageOrderOutItemAnimatorStates.ProcessStart, false, null, null);
        }

        public void StartTimer(float duration)
        {
            if(m_TimerFillBarMaterial == null)
            {
                m_TimerFillBarMaterial = m_TimerFillBarRenderer.material;
            }
            m_TimerFillBarMaterial.DOFloat(1f, m_FillAmountPropertyName, duration).SetEase(Ease.Linear);
        }
        public void ProduceReport()
        {
            m_TimerFillBarMaterial.SetFloat(m_FillAmountPropertyName, 1f);
            m_MachineAnimationHandler.PlayState(StationEngageOrderOutItemAnimatorStates.Produce, false, null, null);
        }

        private void OnProcessComplete()
        {
            ResetCurrentOrder();
            HandleOrder();
        }

        private void OnInstantProcessComplete()
        {
            ResetTimer();
            m_MachineAnimationHandler.PlayState(StationEngageOrderOutItemAnimatorStates.Init, true, null, null);
            OnProcessComplete();
        }

        private void ResetTimer()
        {
            if(m_TimerFillBarMaterial == null)
            {
                m_TimerFillBarMaterial = m_TimerFillBarRenderer.material;
            }
            m_TimerFillBarMaterial.DOKill();
            m_TimerFillBarMaterial.SetFloat(m_FillAmountPropertyName, 0f);
        }
        private void ResetCurrentOrder()
        {
            m_IsHandlingOrder = false;
        }


        [Serializable]
        private class ReportInfo
        {
            public CustomerFirstOrderInfo FirstOrder;
            public GameObject TargetScreenReport;
            public GameObject TargetMagnifyingGlass;
            public GameObject TargetOutputReport;
            public Color TargetFillBarColor;
        }
    }
}