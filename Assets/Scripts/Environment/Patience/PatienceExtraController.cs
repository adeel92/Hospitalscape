using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.Customer;

namespace Isometric.Environment
{
    public class PatienceExtraController : MonoBehaviour
    {
        private const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable]
        private DataPatience m_Data;

        //---Menu Calls---
        private const string MetaMenuCallsFoldOut = "---Menu Calls---";
        [Header("-Station is locked"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsLockedMenu;
        [Header("-Station is unlocked (Not CALLED FIRST TIME)"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsUnlockdMenu;
        [Header("-Unlocking for the first time")]
        [Foldout(MetaMenuCallsFoldOut)] public UnityEvent OnHasUnlockedMenu;
        [Header("-Upgraded any of the properties"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnHasUpgradedMenu;

        //---Gameplay Calls---
        private const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked (Not CALLED FIRST TIME)"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;
        [Header("-Unlocking for the first time"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnHasUnlockedGameplay;
        [Header("-Upgraded any of the properties"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnHasUpgradedGameplay;

        [ContextMenu("SetupForMenu")]
        public void SetupForMenu()
        {
            if (!m_Data.PatienceData.IsUnlocked)
            {
                OnIsLockedMenu?.Invoke();
            }
            else if (m_Data.PatienceData.IsUnlocked && !m_Data.PatienceData.HasJustUnlocked)
            {
                OnIsUnlockdMenu?.Invoke();
            }

            if (m_Data.PatienceData.HasJustUnlocked)
            {
                OnHasUnlockedMenu?.Invoke();
                m_Data.PatienceData.HasJustUnlocked = false;
                m_Data.Save();
            }

            if (m_Data.PatienceData.HasUpgraded)
            {
                OnHasUpgradedMenu?.Invoke();
                m_Data.PatienceData.HasUpgraded = false;
                m_Data.Save();
            }
        }

        [ContextMenu("SetupForGameplay")]
        public void SetupForGameplay()
        {
            if (!m_Data.PatienceData.IsUnlocked)
            {
                OnIsLockedGameplay?.Invoke();
            }
            else if (m_Data.PatienceData.IsUnlocked && !m_Data.PatienceData.HasJustUnlocked)
            {
                float patienceValue = m_Data.PatienceData.PatienceUpgrade[m_Data.PatienceData.CurrentUpgradeIndex];
                float finalPatience = CustomerPatienceManager.GetExtraPatienace() + patienceValue;
                CustomerPatienceManager.SetExtraPatieance(finalPatience);
                OnIsUnlockdGameplay?.Invoke();
            }

            if (m_Data.PatienceData.HasJustUnlocked)
            {
                float patienceValue = m_Data.PatienceData.PatienceUpgrade[m_Data.PatienceData.CurrentUpgradeIndex];
                float finalPatience = CustomerPatienceManager.GetExtraPatienace() + patienceValue;
                CustomerPatienceManager.SetExtraPatieance(finalPatience);
                OnHasUnlockedGameplay?.Invoke();
                m_Data.PatienceData.HasJustUnlocked = false;
                m_Data.Save();
            }

            if (m_Data.PatienceData.HasUpgraded)
            {
                OnHasUpgradedGameplay?.Invoke();
                m_Data.PatienceData.HasUpgraded = false;
                m_Data.Save();
            }

        }
    }
}
