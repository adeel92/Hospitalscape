using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.TaskSystem;
using Isometric.Customer;
using Isometric.Cam;
using Isometric.UI;

namespace Isometric.Environment
{
    public class StationVacuumPatience : MonoBehaviour
    {
        private const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable]
        private DataStation m_Data;
        [SerializeField, Foldout(MetaSetupFoldOut)] TaskTrigger m_TaskTrigger;
        [SerializeField, Foldout(MetaSetupFoldOut)] float m_CoolAirEffectDuration;
        [Foldout(MetaSetupFoldOut)] public UnityEvent OnActivation;
        [Foldout(MetaSetupFoldOut)] public UnityEvent OnDeactivation;
        private Coroutine m_CoolAirEffectStopCorotine;

        //---Menu Calls---
        private const string MetaMenuCallsFoldOut = "---Menu Calls---";
        [Header("-Station is locked"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsLockedMenu;
        [Header("-Station is unlocked (Not CALLED FIRST TIME)"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsUnlockdMenu;
        [Header("-Unlocking for the first time")]
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] Vector2 m_CameraFocusPosition;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraZoom;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraFocusDuration;
        [Foldout(MetaMenuCallsFoldOut)] public UnityEvent OnHasUnlockedMenu;

        //---Gameplay Calls---
        private const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;

        [ContextMenu("SetupForMenu")]
        public void SetupForMenu()
        {
            if (!m_Data.StationData.IsUnlocked)
            {
                OnIsLockedMenu?.Invoke();
            }
            else if (m_Data.StationData.IsUnlocked && !m_Data.StationData.HasJustUnlocked)
            {
                OnIsUnlockdMenu?.Invoke();
            }

            if (m_Data.StationData.HasJustUnlocked)
            {
                CameraController.RegisterFocusCamera(m_CameraFocusPosition, m_CameraZoom, 1.4f,
                () =>
                {
                    UIManager.UIInteractionOff();
                    UIManager.HideMenu(null);
                    CameraController.Interactability(false);
                },
                () =>
                {
                    OnHasUnlockedMenu?.Invoke();
                    CoroutineManager.LateAction(() =>
                    {
                        if (CameraController.NextFocusCamera() == false)
                        {
                            CameraController.SetupForMenu(() =>
                            {
                                UIManager.CheckNextUpdatable();
                            });
                        }

                    }, m_CameraFocusDuration);
                });

                m_Data.StationData.HasJustUnlocked = false;
                m_Data.Save();
            }
        }

        [ContextMenu("SetupForGameplay")]
        public void SetupForGameplay()
        {
            if (!m_Data.StationData.IsUnlocked)
            {
                OnIsLockedGameplay?.Invoke();
            }
            else if (m_Data.StationData.IsUnlocked)
            {
                OnIsUnlockdGameplay?.Invoke();
            }
        }

        private void OnEnable()
        {
            m_TaskTrigger.OnTaskStart += OnTaskStart;
        }

        private void OnDisable()
        {
            m_TaskTrigger.OnTaskStart -= OnTaskStart;
        }

        private void OnTaskStart(TaskTarget taskTarget)
        {
            if (taskTarget.TryGetComponent(out IEnvironmentInteractable interactable))
            {
                if (CustomerPatienceManager.IsSunRaysActivated())
                {
                    CustomerPatienceManager.DeactivateSunRays();

                    OnActivation?.Invoke();

                    if (m_CoolAirEffectStopCorotine != null)
                    {
                        CoroutineManager.StopACoroutine(m_CoolAirEffectStopCorotine);
                    }

                    m_CoolAirEffectStopCorotine = CoroutineManager.LateAction(() => OnDeactivation?.Invoke(), m_CoolAirEffectDuration);
                }

                m_TaskTrigger.SendTaskResult(TaskResult.Success);
            }
            else
            {
                m_TaskTrigger.SendTaskResult(TaskResult.Failed);
            }
        }
    }
}
