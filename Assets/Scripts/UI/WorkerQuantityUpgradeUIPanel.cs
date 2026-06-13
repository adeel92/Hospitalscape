using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
namespace Isometric.UI
{
    public class WorkerQuantityUpgradeUIPanel : MonoBehaviour
    {
        [SerializeField, ReadOnly] WorkerUIManager m_WorkerUIManager;

        [SerializeField] Image m_WorkerOrderTypeImage;
        [SerializeField] Image m_WorkerOrderSymbolImage;
        [SerializeField] GameObject m_SelectedImage;
        private Sprite m_WorkerSprite;
        private Action OnUpgradeCallback;


        public void Setup(WorkerUIManager workerUIManger, Sprite workerOrderTypeSprite, Sprite workerOrderSymbolSprite, Sprite workerSprite, Action onUpgradeCallback)
        {
            m_WorkerUIManager = workerUIManger;
            m_WorkerOrderTypeImage.sprite = workerOrderTypeSprite;
            m_WorkerOrderSymbolImage.sprite = workerOrderSymbolSprite;
            m_WorkerSprite = workerSprite;
            OnUpgradeCallback = onUpgradeCallback;
            m_SelectedImage.SetActive(false);
        }

        public void OnSelected()
        {
            m_SelectedImage.SetActive(true);
            m_WorkerUIManager.SelectWorkerQuantityUpgradeUIPanel(this);
        }

        public void OnUnselected()
        {
            m_SelectedImage.SetActive(false);
        }

        public Sprite OnUpgraded()
        {
            OnUpgradeCallback?.Invoke();
            return m_WorkerSprite;
        }
    }
}
