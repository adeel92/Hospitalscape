using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Isometric.UI
{
    public abstract class UIPopupBase : MonoBehaviour
    {
        public abstract void Setup();
        public abstract void OpenPopup(Action onCompete);
        public abstract void ClosePopup(Action onComplete);
    }
}
