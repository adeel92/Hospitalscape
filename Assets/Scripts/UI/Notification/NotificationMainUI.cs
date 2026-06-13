using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;

namespace Isometric.UI
{
    public class NotificationMainUI : MonoBehaviour
    {
        [SerializeField] bool m_AutoFirstSetup;
        
        [SerializeField] bool m_EditKey;
        [SerializeField, EnableIf(nameof(m_EditKey))] 
        string m_SeeenKey;

        [SerializeField] List<NotificationParentUI> m_NotificationParentUIs;

        public UnityEvent OnNotificationOn;
        public UnityEvent OnNotificationOff;


        private void Start()
        {
            if (m_AutoFirstSetup)
            {
                Setup();
            }
        }

        public void Setup(string seenKey = null, List<NotificationParentUI> notificationParentUIs = null)
        {
            if (seenKey != null)
            {
                m_SeeenKey = seenKey;
            }

            if (DataManager.GetBool(m_SeeenKey, false) == false)
            {
                OnNotificationOn?.Invoke();

                if (notificationParentUIs != null)
                {
                    m_NotificationParentUIs = notificationParentUIs;
                    foreach (var notificationParentUI in notificationParentUIs)
                    {
                        notificationParentUI.AddNotificationMain(this);
                    }
                }
            }
            else
            {
                OnNotificationOff?.Invoke();
            }

            DataManager.SaveData();
        }

        public void OnNotficationSeen()
        {
            DataManager.SetBool(m_SeeenKey, true);
            DataManager.SaveData();
            OnNotificationOff?.Invoke();
            if (m_NotificationParentUIs != null)
            {
                foreach (var notificationParentUI in m_NotificationParentUIs)
                {
                    notificationParentUI.RemoveNotificationMain(this);
                }
            }
        }

    }
}
