using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using Isometric.Data;

namespace Isometric.Environment
{
    public class MainServiceCallWorkerUIController : MonoBehaviour
    {
        [Serializable]
        private class WorkerOrderInfo
        {
            public DataConsumable WorkerOrder;
            public SpriteRenderer WorkerSymbol;
            public Sprite WorkerAvailableSprite;
            public Sprite WorkerBusySprite;
        }

        [SerializeField, ReadOnly] MainServiceController m_MainServiceController;
        [SerializeField] GameObject m_Holder;
        [SerializeField] List<WorkerOrderInfo> m_WorkerOrderInfos;

        private void OnEnable()
        {
            GlobalEventHolder.OnAllWorkerBusy += OnAllWorkerBusy;
            GlobalEventHolder.OnNotAllWorkerBusy += OnNotAllWorkerBusy;
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnAllWorkerBusy -= OnAllWorkerBusy;
            GlobalEventHolder.OnNotAllWorkerBusy -= OnNotAllWorkerBusy;
        }

        public void Setup(MainServiceController mainServiceController)
        {
            m_MainServiceController = mainServiceController;
            SetActiveState(false);
        }

        public void SetupWorkerOrder(DataConsumable firstOrder)
        {
            foreach (var workerOrderInfo in m_WorkerOrderInfos)
            {
                if (workerOrderInfo.WorkerOrder == firstOrder)
                {
                    workerOrderInfo.WorkerSymbol.gameObject.SetActive(true);
                }
                else
                {
                    workerOrderInfo.WorkerSymbol.gameObject.SetActive(false);
                }
            }
        }

        public void CallWorker()
        {
            m_MainServiceController.ServeFirstOrder();
        }

        public void SetActiveState(bool value)
        {
            m_Holder.SetActive(value);
        }

        private void OnAllWorkerBusy(DataConsumable orderType)
        {
            foreach (var workerOrderInfo in m_WorkerOrderInfos)
            {
                if (workerOrderInfo.WorkerOrder == orderType)
                {
                    workerOrderInfo.WorkerSymbol.sprite = workerOrderInfo.WorkerBusySprite;
                    break;
                }
            }
        }

        private void OnNotAllWorkerBusy(DataConsumable orderType)
        {
            foreach (var workerOrderInfo in m_WorkerOrderInfos)
            {
                if (workerOrderInfo.WorkerOrder == orderType)
                {
                    workerOrderInfo.WorkerSymbol.sprite = workerOrderInfo.WorkerAvailableSprite;
                    break;
                }
            }
        }
    }

}
