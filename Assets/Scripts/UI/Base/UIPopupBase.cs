using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.UI
{
    public abstract class UIPopupBase : MonoBehaviour
    {
        public UnityEvent OnPopupOpened;
        public UnityEvent OnPopupClosed;

        public abstract void Setup();
        public abstract void OpenPopup(Action onCompete);
        public abstract void ClosePopup(Action onComplete);
    }
}
