using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric.Environment
{
    public class EnvironmentSetupCaller : MonoBehaviour
    {
        public UnityEvent MenuCalls;
        public UnityEvent GameplayCalls;

        [ContextMenu("CallMenu")]
        public void CallMenu()
        {
            MenuCalls?.Invoke();
        }

        [ContextMenu("CallGameplay")]
        public void CallGameplay()
        {
            GameplayCalls?.Invoke();
        }
        /*public void Start()
        {
            MenuCalls?.Invoke();
            GameplayCalls?.Invoke();
        }*/
    }
}
