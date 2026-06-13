using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace Isometric.UI
{
    public class NotificationParentUI : MonoBehaviour
    {
        [SerializeField] bool m_AutoFirstSetup;

        [Space]
        [SerializeField] List<NotificationParentUI> m_NotificationParentUIs;
        
        [Space]
        [SerializeField] List <NotificationMainUI> m_NotificationMainUIs;

        [Space]
        public bool m_IsNotficationOn = true;
        public UnityEvent OnNotificationOn;
        public UnityEvent OnNotificationOff;

        private void Start()
        {
            if (m_AutoFirstSetup)
            {
                NotficationCheck();
            }
        }

        public void AddNotificationMain(NotificationMainUI notificationMainUI)
        {
            m_NotificationMainUIs ??= new List<NotificationMainUI>();

            if (notificationMainUI != null)
            {
                m_NotificationMainUIs.Add(notificationMainUI);
                m_NotificationMainUIs.RemoveAll((x) => x == null);
                NotficationCheck();

                foreach (var notificationParentUI in m_NotificationParentUIs)
                {
                    notificationParentUI.AddNotificationMain(notificationMainUI);
                }
            }
        }

        public void RemoveNotificationMain(NotificationMainUI notificationMainUI)
        {
            m_NotificationMainUIs ??= new List<NotificationMainUI>();

            if (notificationMainUI != null)
            {
                m_NotificationMainUIs.Remove(notificationMainUI);
                m_NotificationMainUIs.RemoveAll((x) => x == null);

                NotficationCheck();

                foreach (var notificationParentUI in m_NotificationParentUIs)
                {
                    notificationParentUI.RemoveNotificationMain(notificationMainUI);
                }
            }
        }

        public void NotficationCheck()
        {
            if (m_NotificationMainUIs != null && m_NotificationMainUIs.Count > 0)
            {
                if (m_IsNotficationOn == false)
                {
                    OnNotificationOn?.Invoke();
                    m_IsNotficationOn = true;
                }
            }
            else
            {
                if (m_IsNotficationOn == true)
                {
                    OnNotificationOff?.Invoke();
                    m_IsNotficationOn = false;
                }
            }
        }

        public void AddNotificationParent(NotificationParentUI notificationParentUI)
        {
            m_NotificationParentUIs ??= new List<NotificationParentUI>();

            if (notificationParentUI != null)
            {
                m_NotificationParentUIs.Add(notificationParentUI);
                m_NotificationParentUIs.RemoveAll((x) => x == null);

                foreach (var notificationMainUI in m_NotificationMainUIs)
                {
                    notificationParentUI.AddNotificationMain(notificationMainUI);
                }
            }
        }

        public void RemoveNotificationParent(NotificationParentUI notificationParentUI)
        {
            m_NotificationParentUIs ??= new List<NotificationParentUI>();

            if (notificationParentUI != null)
            {
                m_NotificationParentUIs.Remove(notificationParentUI);
                m_NotificationParentUIs.RemoveAll((x) => x == null);
            }
        }
    }
}
