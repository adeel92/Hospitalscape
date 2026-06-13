using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Isometric.UI
{
    [RequireComponent(typeof(Image))]
    public class SlideButtonUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] float m_OnOffDuration;
        [SerializeField] RectTransform m_Toggle;
        [SerializeField] Vector2 m_ToggleOnPosition;
        [SerializeField] Vector2 m_ToggleOffPosition;
        [SerializeField] Image m_Fill;
        [SerializeField] float m_FillOnValue;
        [SerializeField] float m_FillOffValue;
        [Space]
        [SerializeField] UnityEvent OnToggleOn;
        [SerializeField] UnityEvent OnToggleOff;

        bool m_IsOn = false;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_IsOn)
            {
                SetOff();
            }
            else
            {
                SetOn();
            }
        }

        public void SetOn(bool callEvent = true)
        {
            m_IsOn = true;
            m_Toggle.DOKill();
            m_Toggle.DOAnchorPos(m_ToggleOnPosition, m_OnOffDuration);
            m_Fill.DOKill();
            m_Fill.DOFillAmount(m_FillOnValue, m_OnOffDuration);

            if(callEvent) OnToggleOn?.Invoke();
        }

        public void SetOff(bool callEvent = true)
        {
            m_IsOn = false;
            m_Toggle.DOKill();
            m_Toggle.DOAnchorPos(m_ToggleOffPosition, m_OnOffDuration);
            m_Fill.DOKill();
            m_Fill.DOFillAmount(m_FillOffValue, m_OnOffDuration);

            if(callEvent) OnToggleOff?.Invoke();
        }
    }
}
