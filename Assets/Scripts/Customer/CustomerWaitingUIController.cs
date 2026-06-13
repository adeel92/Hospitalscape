using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Isometric.Data;

namespace Isometric.Customer
{
    public class CustomerWaitingUIController : MonoBehaviour
    {
        [Serializable]
        private class WorkerOrderInfo
        {
            public DataConsumable WorkerOrder;
            public GameObject WorkerOrderSymbol;
        }

        [SerializeField] List<WorkerOrderInfo> m_WorkerOrderInfos;
        [SerializeField] GameObject m_CafeOrderSymbol;
        [SerializeField] GameObject m_UndecidedOrderSymbol;
        [SerializeField] GameObject m_WaitBarHolder;
        [SerializeField] RectTransform m_ShakeRectTransform;
        [SerializeField] float m_ShakeDuration;
        [SerializeField] float m_ShakeStrength;
        [SerializeField] Image m_WaitBarFill;
        [SerializeField] ParticleSystem m_TimeFrozenEffect;
        [SerializeField] ParticleSystem m_HeatEffect;


        public void SetupForSalon(DataConsumable firstOrder, bool isOrderUndecided)
        {
            SetActiveState(false);
            m_TimeFrozenEffect.gameObject.SetActive(false);
            m_HeatEffect.gameObject.SetActive(false);

            m_CafeOrderSymbol.SetActive(false);
            if(isOrderUndecided)
            {
                 m_UndecidedOrderSymbol.SetActive(true);
                foreach (var workerOrderInfo in m_WorkerOrderInfos)
                {
                    workerOrderInfo.WorkerOrderSymbol.SetActive(false);
                }
            }
            else
            {
                m_UndecidedOrderSymbol.SetActive(false);
                foreach (var workerOrderInfo in m_WorkerOrderInfos)
                {
                    if (workerOrderInfo.WorkerOrder == firstOrder)
                    {
                        workerOrderInfo.WorkerOrderSymbol.SetActive(true);
                    }
                    else
                    {
                        workerOrderInfo.WorkerOrderSymbol.SetActive(false);
                    }
                }
            }
        }

        public void SetupForCafe(bool isOrderUndecided)
        {
            SetActiveState(false);
            m_TimeFrozenEffect.gameObject.SetActive(false);
            m_HeatEffect.gameObject.SetActive(false);

            if (isOrderUndecided)
            {
                m_CafeOrderSymbol.SetActive(false);
                m_UndecidedOrderSymbol.SetActive(true);

            }
            else
            {
                m_CafeOrderSymbol.SetActive(true);
                m_UndecidedOrderSymbol.SetActive(false);
            }

            foreach (var workerOrderInfo in m_WorkerOrderInfos)
            {
                workerOrderInfo.WorkerOrderSymbol.SetActive(false);
            }
        }

        public void SetActiveState(bool value)
        {
            m_WaitBarHolder.SetActive(value);
        }

        public void SetWaitingFill(float fillAmount)
        {
            m_WaitBarFill.fillAmount = fillAmount;
        }

        public void SetWaitingColor(Color color)
        {
            m_WaitBarFill.color = color;
        }

        public void StartVibrating()
        {
            m_ShakeRectTransform.DOShakeAnchorPos(m_ShakeDuration, m_ShakeStrength, 10, 90, false, false).SetLoops(-1, LoopType.Yoyo);
        }

        public void StopVibrating()
        {
            m_ShakeRectTransform.DOKill();
        }

        public void PlayTimeFrozeEffect()
        {
            m_TimeFrozenEffect.gameObject.SetActive(true);
            m_TimeFrozenEffect.Play();
        }

        public void StopTimeFrozeEffect()
        {
            m_TimeFrozenEffect.Stop();
            m_TimeFrozenEffect.gameObject.SetActive(false);
        }

        public void PlayHeatEffect()
        {
            m_HeatEffect.gameObject.SetActive(true);
            m_HeatEffect.Play();
        }

        public void StopHeatEffect()
        {
            m_HeatEffect.Stop();
            m_HeatEffect.gameObject.SetActive(false);
        }
    }
}
