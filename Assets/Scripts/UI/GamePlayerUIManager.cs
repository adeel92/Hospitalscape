using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Isometric.PathSystem
{
    public class GamePlayerUIManager : MonoBehaviour
    {
        

        public void StopTask()
        {
            GlobalEventHolder.OnCurrentTaskTargetCancle?.Invoke();
        }

    }
}
