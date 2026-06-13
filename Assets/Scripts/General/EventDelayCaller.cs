using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Isometric
{
    public class EventDelayCaller : MonoBehaviour
    {
        [SerializeField] float m_Delay;
        [SerializeField] UnityEvent OnDelayCallback;

        public void CallDelayEvent()
        {
            StartCoroutine(LateCall());
        }

        IEnumerator LateCall()
        {
            yield return new WaitForSeconds(m_Delay);
            OnDelayCallback?.Invoke();
        }
    }
}
