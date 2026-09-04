using System;
using System.Collections.Generic;
using Isometric.Data;
using UnityEngine;


namespace Isometric.Environment
{
    public class OrderItemVisualSelector : MonoBehaviour
    {
        [Header("---First Order Based---")]
        [SerializeField] List<FirstOrderBasedVisualInfo> m_FirstOrderBasedVisualInfos = new();


        public void SelectVisual(CustomerFirstOrderInfo firstOrder)
        {
            foreach(FirstOrderBasedVisualInfo firstOrderBasedVisualInfo in m_FirstOrderBasedVisualInfos)
            {
                GameObject visualObject = firstOrderBasedVisualInfo.targetObject;
                if(firstOrderBasedVisualInfo.FirstOrderInfo.OrderConsumable == firstOrder.OrderConsumable)
                {
                    visualObject.SetActive(true);
                }
                else
                {
                    visualObject.SetActive(false);
                }
            }
        }


        [Serializable]
        public class FirstOrderBasedVisualInfo
        {
            public CustomerFirstOrderInfo FirstOrderInfo;
            public GameObject targetObject;
        }
    }
}