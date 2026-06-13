using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;

namespace Isometric.Tutorial
{
    public class HintManager : MonoBehaviour
    {
        public static HintManager s_Instance;

        [Serializable]
        private class HintInfo
        {
            public HintKey HintKey;
            public bool HasDataConsumableKey;
            [AllowNesting, ShowIf(nameof(HasDataConsumableKey))]
            public DataConsumable DataConsumable;
            public UnityEvent OnHintCalled;
        }

        [SerializeField] List<HintInfo> m_HintsInfo;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        private void OnEnable()
        {
            GlobalEventHolder.OnHintByKey += PlayHint;
            GlobalEventHolder.OnHintByConsumable += PlayHint;
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnHintByKey -= PlayHint;
            GlobalEventHolder.OnHintByConsumable -= PlayHint;
        }

        public static void PlayHint(HintKey hintKey)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            HintInfo hintInfo = s_Instance.m_HintsInfo.Find((x) => x.HintKey == hintKey);
            if (hintInfo != null)
            {
                hintInfo.OnHintCalled?.Invoke();
            }
        }

        public static void PlayHint(DataConsumable dataConsumable)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            HintInfo hintInfo = s_Instance.m_HintsInfo.Find((x) => x.DataConsumable == dataConsumable);
            if (hintInfo != null)
            {
                hintInfo.OnHintCalled?.Invoke();
            }
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(HintManager) + " is null");
        }
    }

    public enum HintKey
    {
        WorkerOrderFacial,
        WorkerOrderHairCut,
        WorkerOrderHairDye,
        WorkerOrderMakeup,
        WorkerOrderManicurePedicure,
        SaladFood,
        SaladBarFood,
        CurlyFriesFood,
        PopopCornFood,
        FriesFood,
        FishCookedFood,
        FishRawFood,
        IceCreamFood,
        SodaFood,
        Performance
    }
}
