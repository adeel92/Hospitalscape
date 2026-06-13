using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.UI;

namespace Isometric.Customer
{
    public class CustomerPatienceManager : MonoBehaviour
    {
        private static CustomerPatienceManager s_Instnace;

        [SerializeField] bool m_EditValues = false;
        [SerializeField, EnableIf(nameof(m_EditValues))] float m_ExtraPatienace = 0;
        [SerializeField, EnableIf(nameof(m_EditValues))] bool m_HasSofaPatieance = false;
        [SerializeField, EnableIf(nameof(m_EditValues))] float m_SofaPatienaceCoolDownPercentage = 1;

        [Header("---Sun Rays---")]
        [Header("-Higher the value more quickly patience depletes")]
        [SerializeField] float m_HotSunRaysPatienceCoolDownMultipier;
        private bool m_HotSunRaysActivated;
        [SerializeField] ParticleSystem m_SunRaysEffect;

        private void Awake()
        {
            if (s_Instnace == null)
            {
                s_Instnace = this;
            }
        }

        public static void Setup()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instnace.m_HotSunRaysActivated = false;
            DataLevel dataLevel = DataManager.GetCurrentDataLevel();

            if (dataLevel != null)
            {
                s_Instnace.StartCoroutine(s_Instnace.SunRaysChecker(dataLevel));
            }
        }

        #region Extra Patience
        public static float GetExtraPatienace()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return 0;
            }

            return s_Instnace.m_ExtraPatienace;
        }

        public static void SetExtraPatieance(float value)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instnace.m_ExtraPatienace = value;
        }
        #endregion

        #region Sofa Patience
        public static void SetHasSofaPatience(bool value)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instnace.m_HasSofaPatieance = value;
        }

        public static bool HasSofaPatience()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return false;
            }

            return s_Instnace.m_HasSofaPatieance;
        }

        public static float GetSofaPatienaceCoolDown()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return 1;
            }

            return s_Instnace.m_SofaPatienaceCoolDownPercentage;
        }

        public static void SetSofaPatienaceCoolDown(float value)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instnace.m_SofaPatienaceCoolDownPercentage = value;
        }
        #endregion

        #region Sun Rays Patience

        IEnumerator SunRaysChecker(DataLevel dataLevel)
        {
            foreach (var patienceSunRaysData in dataLevel.PatienceSunRaysData)
            {
                yield return new WaitForSeconds(patienceSunRaysData.ActivationDelay);
                ActivateSunRays();
                yield return new WaitWhile(() => m_HotSunRaysActivated == true);
            }
        }

        public static void ActivateSunRays()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            GlobalEventHolder.OnPatienceSunRays?.Invoke(true);
            s_Instnace.m_HotSunRaysActivated = true;
            s_Instnace.m_SunRaysEffect.Play();
            if (UIManager.GetPopup<GameplayUIManager>() != null) UIManager.GetPopup<GameplayUIManager>().SetActiveWeatherSymbol(true);
            else Debug.LogWarning(nameof(GameplayUIManager) + " is null");
        }

        public static void DeactivateSunRays()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            GlobalEventHolder.OnPatienceSunRays?.Invoke(false);
            s_Instnace.m_HotSunRaysActivated = false;
            s_Instnace.m_SunRaysEffect.Stop();
            if (UIManager.GetPopup<GameplayUIManager>() != null) UIManager.GetPopup<GameplayUIManager>().SetActiveWeatherSymbol(false);
            else Debug.LogWarning(nameof(GameplayUIManager) + " is null");
        }

        public static bool IsSunRaysActivated()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return false;
            }

            return s_Instnace.m_HotSunRaysActivated;
        }

        public static float GetSunRaysPatienaceCoolDown()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return 1;
            }

            if (s_Instnace.m_HotSunRaysActivated)
            {
                return s_Instnace.m_HotSunRaysPatienceCoolDownMultipier;
            }
            else
            {
                return 1;
            }
        }
        #endregion

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(CustomerPatienceManager) + " is null");
        }
    }
}
