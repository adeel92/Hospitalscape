using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Isometric
{
    public class SpriteFillConditionalController : MonoBehaviour
    {
        [Serializable]
        private class SpriteFillInfo
        {
            public float MatchValue;
            public List<SpriteFillController> SpriteFillControllers;
        }

        [SerializeField] List<SpriteFillInfo> m_SpriteFillInfos;


        public void SetFillAmount(float fillAmount, float MatchValue)
        {
            foreach (var spriteFillInfo in m_SpriteFillInfos)
            {
                if (spriteFillInfo.MatchValue == MatchValue)
                {
                    foreach (var spriteFillController in spriteFillInfo.SpriteFillControllers)
                    {
                        spriteFillController.SetFillAmount(fillAmount);
                    }
                }
            }
        }

        public void StartFill(float duration, float MatchValue)
        {
            foreach (var spriteFillInfo in m_SpriteFillInfos)
            {
                if (spriteFillInfo.MatchValue == MatchValue)
                {
                    foreach (var spriteFillController in spriteFillInfo.SpriteFillControllers)
                    {
                        spriteFillController.StartFill(duration);
                    }
                }
            }
        }
    }
}
