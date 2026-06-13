using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.TaskSystem;
using Isometric.Cam;
using Isometric.UI;
using Isometric.Customer;
using Isometric.PathSystem;

namespace Isometric.Environment
{
    public class CounterTableController : MonoBehaviour
    {
        //---Setup---
        private const string MetaSetupFoldOut = "---Setup---";
        [SerializeField, Foldout(MetaSetupFoldOut), Expandable] 
        private DataStation m_Data;

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
        [Header("-Upgraded any of the properties"), Foldout(MetaMenuCallsFoldOut)]
        public UnityEvent OnHasUpgradedMenu;

        //---Gameplay Calls---
        private const string MetaGameplayCallsFoldOut = "---Gameplay Calls---";
        [Header("-Station is locked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsLockedGameplay;
        [Header("-Station is unlocked"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnIsUnlockdGameplay;
        [Header("-Upgraded any of the properties"), Foldout(MetaGameplayCallsFoldOut)]
        public UnityEvent OnHasUpgradedGameplay;


        private const string MetaInteractiveFoldOut = "---Interactive---";
        [SerializeField, Foldout(MetaInteractiveFoldOut)] float m_OrderDecidingDelay;
        [SerializeField, Foldout(MetaInteractiveFoldOut)] PathNode m_StandingNode;
        [SerializeField, Foldout(MetaInteractiveFoldOut)] float m_CaptureDistance;
        [SerializeField, Foldout(MetaInteractiveFoldOut)] float m_ExitDistance;
        [SerializeField, SortingLayer, Foldout(MetaInteractiveFoldOut)] string m_SittingSortingLayer;
        [SerializeField, Foldout(MetaInteractiveFoldOut)] int m_SittingSortingOrder;
        [SerializeField, Foldout(MetaInteractiveFoldOut), ReadOnly] CustomerSalonController m_CurrentSalonCustomer;
        [SerializeField, Foldout(MetaInteractiveFoldOut), ReadOnly] CustomerCafeController m_CurrentCafeCustomer;


        Coroutine m_DectectionUpdate = null;


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
                OnIsUnlockdGameplay?.Invoke();
            }

            if (m_Data.StationData.HasUpgraded)
            {
                OnHasUpgradedMenu?.Invoke();
                m_Data.StationData.HasUpgraded = false;
                m_Data.Save();
            }

            if(m_Data.StationData.IsUnlocked)
            {
                if (m_DectectionUpdate != null)
                {
                    StopCoroutine(m_DectectionUpdate);
                    m_DectectionUpdate = null;
                }
                m_DectectionUpdate = StartCoroutine(DectectionUpdate());
            }
        }


        private IEnumerator DectectionUpdate()
        {
            float sqrCaptureDistance = m_CaptureDistance * m_CaptureDistance;

            while (true)
            {
                yield return null;

                if (m_CurrentSalonCustomer == null && m_CurrentCafeCustomer == null)
                {
                    List<CustomerSalonController> salonCustomers = CustomerManager.GetAllSalonCustomer();

                    foreach (var salonCustomer in salonCustomers)
                    {
                        if (salonCustomer != null 
                            && salonCustomer.IsCustomerFirstOrderUndecided()
                            && !salonCustomer.IsCustomerOnSalonChair())
                        {
                            float sqrDist = (salonCustomer.transform.position - m_StandingNode.transform.position).sqrMagnitude;
                            
                            if (sqrDist <= sqrCaptureDistance)
                            {
                                m_CurrentSalonCustomer = salonCustomer;
                                m_CurrentSalonCustomer.transform.SetParent(m_StandingNode.transform);
                                m_CurrentSalonCustomer.transform.localPosition = Vector3.zero;
                                m_CurrentSalonCustomer.SetSortingLayer(m_SittingSortingLayer, m_SittingSortingOrder);
                                m_CurrentSalonCustomer.StandAtTheCounter(this);
                                yield break;
                            }
                        }
                    }

                    List<CustomerCafeController> cafeCustomers = CustomerManager.GetAllCafeCustomer();

                    foreach (var cafeCustomer in cafeCustomers)
                    {
                        if (cafeCustomer != null && !cafeCustomer.IsCustomerOnCafeChair())
                        {
                            float sqrDist = (cafeCustomer.transform.position - m_StandingNode.transform.position).sqrMagnitude;

                            if (sqrDist <= sqrCaptureDistance)
                            {
                                m_CurrentCafeCustomer = cafeCustomer;
                                m_CurrentCafeCustomer.transform.SetParent(m_StandingNode.transform);
                                m_CurrentCafeCustomer.transform.localPosition = Vector3.zero;
                                m_CurrentCafeCustomer.SetSortingLayer(m_SittingSortingLayer, m_SittingSortingOrder);
                                m_CurrentCafeCustomer.StandAtTheCounter(this);
                                yield break;
                            }
                        }
                    }
                }

            }
        }

        public void ReStandAtTheCounter()
        {
            if(m_CurrentSalonCustomer != null)
            {
                m_CurrentSalonCustomer.transform.SetParent(m_StandingNode.transform);
                m_CurrentSalonCustomer.transform.localPosition = Vector3.zero;
                m_CurrentSalonCustomer.SetSortingLayer(m_SittingSortingLayer, m_SittingSortingOrder);
            }

            if(m_CurrentCafeCustomer != null)
            {
                m_CurrentCafeCustomer.transform.SetParent(m_StandingNode.transform);
                m_CurrentCafeCustomer.transform.localPosition = Vector3.zero;
                m_CurrentCafeCustomer.SetSortingLayer(m_SittingSortingLayer, m_SittingSortingOrder);
            }
        }

        public void StandAtTheCounter(Action onWaitComplete)
        {
            CoroutineManager.LateAction(() =>
            {
                onWaitComplete?.Invoke();
            }, m_OrderDecidingDelay);
        }

        public void CustomerRemoved()
        {
            m_CurrentSalonCustomer = null;
            m_CurrentCafeCustomer = null;

            if (m_DectectionUpdate != null)
            {
                StopCoroutine(m_DectectionUpdate);
                m_DectectionUpdate = null;
            }
            m_DectectionUpdate = StartCoroutine(DectectionUpdate());
        }

        public PathNode GetStandingPathNode()
        {
            return m_StandingNode;
        }

        public float GetExitDistance()
        {
            return m_ExitDistance;
        }


        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(m_StandingNode == null) return;
            
            DrawCircle(Color.blue, m_CaptureDistance);
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
                //Vector3 currentPoint = transform.position + new Vector3(x, y, 0f);
                Vector3 currentPoint = m_StandingNode.transform.position + new Vector3(x, y, 0f);

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
#endif
    }
}
