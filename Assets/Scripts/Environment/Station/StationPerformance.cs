using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.Customer;
using Isometric.Cam;
using Isometric.UI;

namespace Isometric.Environment
{
    public class StationPerformance : MonoBehaviour
    {

        /*const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable]
        private DataStation m_Data;
        [SerializeField, Foldout(MetaSetupFoldOut)] 
        private bool m_UsePhysicsBasedDectection;
        [SerializeField, Foldout(MetaSetupFoldOut), ShowIf(nameof(m_UsePhysicsBasedDectection))]
        private GameObject m_Trigger;
        [SerializeField, Foldout(MetaSetupFoldOut), HideIf(nameof(m_UsePhysicsBasedDectection))]
        private float m_CaptureDistance;
        [SerializeField, Foldout(MetaSetupFoldOut), HideIf(nameof(m_UsePhysicsBasedDectection))]
        private float m_ExitDistance;

        private Coroutine m_DectectionUpdate;
        public CustomerSalonController CurrentCustomer => m_CurrentCustomer;
        [SerializeField, ReadOnly, Foldout(MetaSetupFoldOut)] CustomerSalonController m_CurrentCustomer;

        //---Menu Calls---
        const string MetaMenuCallsFoldOut = "---Menu Calls---";
        [Header("-Station is locked"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsLockedMenu;
        [Header("-Station is unlocked (Not CALLED FIRST TIME)"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnIsUnlockdMenu;
        [Header("-Unlocking for the first time")]
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] Vector2 m_CameraFocusPosition;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraZoom;
        [SerializeField, Foldout(MetaMenuCallsFoldOut)] float m_CameraFocusDuration;
        [Foldout(MetaMenuCallsFoldOut)] public UnityEvent OnHasUnlockedMenu;
        [Header("-Upgraded any of the properties"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnHasUpgradedMenu;

        //---Gameplay Calls---
        const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;
        [Header("-Upgraded any of the properties"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnHasUpgradedGameplay;

        //---Upgrade Properties---
        const string MetaUpgradePropertiesFoldOut = "---Upgrade Properties---";
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] float m_DurationProperty;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_CostProperty;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] Transform m_PerformancePosition;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut), SortingLayer] string m_PerformanceSortingLayer;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] int m_PerformanceSortingOrder;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] UnityEvent OnStart;
        [SerializeField, Foldout(MetaUpgradePropertiesFoldOut)] UnityEvent OnComplete;

        


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

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedMenu?.Invoke();
                m_Data.StationData.HasUpgraded = false;
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
                if(m_UsePhysicsBasedDectection)
                {
                    if (m_Trigger) m_Trigger.SetActive(true);
                }
                else
                {
                    if (m_Trigger) m_Trigger.SetActive(false);

                    if (m_DectectionUpdate != null)
                    {
                        StopCoroutine(m_DectectionUpdate);
                        m_DectectionUpdate = null;
                    }
                    m_DectectionUpdate = StartCoroutine(DectectionUpdate());
                }
                OnIsUnlockdGameplay?.Invoke();
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedGameplay?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }

            StationUpgrade upgradeDuration = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Duration);
            if (upgradeDuration != null)
            {
                m_DurationProperty = upgradeDuration.Upgrade[upgradeDuration.CurrentUpgradeIndex];
            }

            StationUpgrade upgradeCost = m_Data.StationData.Upgrades.Find((x) => x.UpgradeType == PropertyUpgradeType.Cost);
            if (upgradeDuration != null)
            {
                m_CostProperty = Mathf.RoundToInt(upgradeCost.Upgrade[upgradeCost.CurrentUpgradeIndex]);
            }
        }

        public void OnCustomerEnter(GameObject target)
        {
            if (m_CurrentCustomer == null)
            {
                if (target.transform.parent.TryGetComponent(out CustomerSalonController customer))
                {
                    if (m_CurrentCustomer.IsCustomerOnSalonChair())
                    {
                        m_CurrentCustomer = customer;
                        m_CurrentCustomer.transform.SetParent(m_PerformancePosition);
                        m_CurrentCustomer.transform.localPosition = Vector3.zero;
                        m_CurrentCustomer.SetSortingLayer(m_PerformanceSortingLayer, m_PerformanceSortingOrder);
                        customer.StandOnPerformanceStation(this);
                        m_Trigger.SetActive(false);
                    }
                }
            }
        }

        private IEnumerator DectectionUpdate()
        {
            if (m_UsePhysicsBasedDectection) yield break;

            float sqrCaptureDistance = m_CaptureDistance * m_CaptureDistance;

            while (true)
            {
                yield return null;

                if (m_CurrentCustomer == null)
                {
                    List<CustomerSalonController> customers = CustomerManager.GetAllSalonCustomer();
                    foreach (var customer in customers)
                    {
                        if (customer != null && customer.IsCustomerOnSalonChair())
                        {
                            float sqrDist = (customer.transform.position - transform.position).sqrMagnitude;

                            if (sqrDist <= sqrCaptureDistance)
                            {
                                m_CurrentCustomer = customer;
                                m_CurrentCustomer.transform.SetParent(m_PerformancePosition);
                                m_CurrentCustomer.transform.localPosition = Vector3.zero;
                                m_CurrentCustomer.SetSortingLayer(m_PerformanceSortingLayer, m_PerformanceSortingOrder);
                                customer.StandOnPerformanceStation(this);
                                yield break;
                            }
                        }
                    }
                }

            }
        }

        public void StartPerformance()
        {
            OnStart?.Invoke();
            CoroutineManager.LateAction(() =>
            {
                OnComplete?.Invoke();
                m_CurrentCustomer.CustomerOutOnStationDone(m_CostProperty);
                RemoveCustomer();
            }, m_DurationProperty);
        }

        public void RemoveCustomer()
        {
            m_CurrentCustomer = null;

            if (m_UsePhysicsBasedDectection)
            {
                if (m_Trigger) m_Trigger.SetActive(true);
            }
            else
            {
                if (m_DectectionUpdate != null)
                {
                    StopCoroutine(m_DectectionUpdate);
                    m_DectectionUpdate = null;
                }
                m_DectectionUpdate = StartCoroutine(DectectionUpdate());
            }
        }

        public float GetExitDistance()
        {
            return m_ExitDistance;
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (m_UsePhysicsBasedDectection == false)
            {
                DrawCircle(Color.blue, m_CaptureDistance);
            }

            DrawCircle(Color.red, m_ExitDistance);
        }

        private void DrawCircle(Color color, float radius)
        {
            Gizmos.color = color;

            Vector3 prevPoint = Vector3.zero;
            Vector3 firstPoint = Vector3.zero;

            float angleStep = 360f / 64;

            for (int i = 0; i <= 64; i++)
            {
                float angle = Mathf.Deg2Rad * (i * angleStep);
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                Vector3 currentPoint = transform.position + new Vector3(x, y, 0f);

                if (i > 0)
                {
                    Gizmos.DrawLine(prevPoint, currentPoint);
                }
                else
                {
                    firstPoint = currentPoint;
                }

                prevPoint = currentPoint;
            }

            Gizmos.DrawLine(prevPoint, firstPoint);
        }
#endif*/
    }
}
