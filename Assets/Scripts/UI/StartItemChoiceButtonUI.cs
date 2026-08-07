using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using System;
using UnityEngine.Events;

namespace Isometric.UI
{
    public class StartItemChoiceButtonUI : MonoBehaviour
    {
        [ReadOnly] [SerializeField] int m_ChoiceIndex;
        [SerializeField] Image m_PreviewImage;
        [SerializeField] Button m_ChoiceButton;
        [SerializeField] GameObject m_TickObj;

        public int ChoiceIndex => m_ChoiceIndex;

        public void Setup(int index, Sprite previewSprite, UnityAction buttonOnClickListener)
        {
            m_ChoiceIndex = index;
            m_PreviewImage.sprite = previewSprite;
            m_ChoiceButton.onClick.RemoveAllListeners();
            m_ChoiceButton.onClick.AddListener(buttonOnClickListener);
        }

        public void Select()
        {
            m_TickObj.SetActive(true);
        }
        public void Deselect()
        {
            m_TickObj.SetActive(false);
        }
    }
}
