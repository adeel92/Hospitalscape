using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;
using Isometric.Sound;

namespace Arc
{
    [RequireComponent(typeof(Button))]
    public class ButtonAnimationDoTween : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public ButtonAnimationDoTweenSettings Settings;

        public UnityEvent OnClickDown;
        public UnityEvent OnClickUp;

        private Tween m_ScaleTween = null;
        private Vector3 m_OriginalScale;

        private void Awake()
        {
            m_OriginalScale = transform.localScale;
        }

        private void OnDisable()
        {
            if (m_ScaleTween != null)
            {
                m_ScaleTween.Kill();
                m_ScaleTween = null;
            }

            transform.localScale = m_OriginalScale;
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            OnClickDown?.Invoke();
            SoundManager.PlaySound(Settings.OnClickDownSound);

            if (m_ScaleTween != null)
            {
                m_ScaleTween.Kill();
                m_ScaleTween = null;
            }

            m_ScaleTween = transform.DOScale(m_OriginalScale * Settings.OnClickDownScaleMultiplier, Settings.OnClickDownDuration).SetEase(Settings.OnClickDownEase).SetUpdate(Settings.IsTimeIndpendent);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnClickUp?.Invoke();
            SoundManager.PlaySound(Settings.OnClickUpSound);

            if (m_ScaleTween != null)
            {
                m_ScaleTween.Kill();
                m_ScaleTween = null;
            }

            if (Settings.UseCurve)
            {
                m_ScaleTween = transform.DOScale(m_OriginalScale, Settings.OnClickUpDuration).SetEase(Settings.OnClickUpEaseCurve).SetUpdate(Settings.IsTimeIndpendent);
            }
            else
            {
                m_ScaleTween = transform.DOScale(m_OriginalScale, Settings.OnClickUpDuration).SetEase(Settings.OnClickUpEase).SetUpdate(Settings.IsTimeIndpendent);
            }
        }
    }
}
