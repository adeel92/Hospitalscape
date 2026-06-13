using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;
using Isometric.PathSystem;
using Isometric.Data;
using Isometric.UI;

namespace Isometric.Customer
{
    public class CustomerManager : MonoBehaviour
    {
        private static CustomerManager s_Instance;

        public static Transform ParentTransfomr { get { return s_Instance != null ? s_Instance.transform : null; } }
        public static bool IsTimeFrozeBoosterActivated { get; private set; }
        public static bool IsCustomerWaitFrozen { get; private set; }
        public static bool CanAcquireDoNotLoseCustomerLevelChance { get; private set; }

        [Header("---Testing---")]
        [SerializeField] bool m_TestCustomerInLevel = false;
        [ShowIf(nameof(m_TestCustomerInLevel)), SerializeField] DataLevel m_TestDataLevel;

        [Header("---Setup---")]
        [SerializeField] PathNode m_StartNode;
        [SerializeField] PathNode m_ExitNode;
        [SerializeField, ReadOnly] DataLevel m_DataLevel;

        [SerializeField] QueueInfo m_SofaQueue;

        [SerializeField, NonReorderable] List<QueueInfo> m_CustomerQueue;
        [SerializeField, ReadOnly] List<CustomerSalonController> m_CurrentSalonCustomers;
        [SerializeField, ReadOnly] List<CustomerCafeController> m_CurrentCafeCustomers;

        //[SerializeField, NonReorderable] List<QueueInfo> m_CafeQueue;

        [SerializeField, ReadOnly] int m_CustomerCounter = 0;
        [SerializeField, ReadOnly] int m_ExtraCustomer = 0;

        [Header("---Customer Parameter---")]
        [SerializeField] float m_CustomerWalkSpeed = 4;

        Coroutine m_CustomerEntering = null;
        //Coroutine m_CafeCustomerEntering = null;

        private bool m_ShoudGenerateCustomer = true;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
        }

        private void OnEnable()
        {
            GlobalEventHolder.OnTimeFrozeBooster += OnTimeFrozeBooster;
            GlobalEventHolder.OnCustomerWaitFreeze += OnCustomerWaitFreeze;
            GlobalEventHolder.OnGameWon += OnGamesEnd;
            GlobalEventHolder.OnGameLost += OnGamesEnd;
        }

        private void OnDisable()
        {
            GlobalEventHolder.OnTimeFrozeBooster -= OnTimeFrozeBooster;
            GlobalEventHolder.OnCustomerWaitFreeze -= OnCustomerWaitFreeze;
            GlobalEventHolder.OnGameWon -= OnGamesEnd;
            GlobalEventHolder.OnGameLost -= OnGamesEnd;
        }

        public static void Setup()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }
            
            IsTimeFrozeBoosterActivated = false;

            CanAcquireDoNotLoseCustomerLevelChance = UIManager.CanAcquireDoNotLoseCustomerLevelChance();

            s_Instance.m_DataLevel = s_Instance.m_TestCustomerInLevel ? s_Instance.m_TestDataLevel : DataManager.GetCurrentDataLevel();
            UnityEngine.Random.InitState(DateTime.Now.Millisecond);

            s_Instance.m_CurrentSalonCustomers = new List<CustomerSalonController>();
            s_Instance.m_CurrentCafeCustomers = new List<CustomerCafeController>();

            s_Instance.m_CustomerCounter = 0;

            s_Instance.StopCustomerEntering();
            //s_Instance.StopCafeCustomerEntering();

            s_Instance.m_CustomerEntering = s_Instance.StartCoroutine(s_Instance.CustomerEntering());
            
            //s_Instance.m_CafeCustomerEntering = s_Instance.StartCoroutine(s_Instance.CafeCustomerEntering());
        }

        #region Customers
        private void StopCustomerEntering()
        {
            if (m_CustomerEntering != null)
            {
                StopCoroutine(m_CustomerEntering);
                m_CustomerEntering = null;
            }
        }

        IEnumerator CustomerEntering()
        {
            bool hasCusomterConstraint = false;
            int cusomterConstraintValue = 0;

            foreach (var constraintInfo in m_DataLevel.LevelConstraintInfos)
            {
                if (constraintInfo.ConstraintType == LevelConstraintType.NumberOfCustomers)
                {
                    hasCusomterConstraint = true;
                    cusomterConstraintValue = constraintInfo.NumberOfCustomers + m_ExtraCustomer;
                    break;
                }
            }

            CustomerSequenceInfo sequenceInfo = null;
            List<CustomerData> customersData = null;

            if (m_DataLevel.CustomerSequence == CustomerSequenceType.DefainedSquence)
            {
                List<CustomerSequenceInfo> sequencesInfo = m_DataLevel.CustomerSequencesInfo.CustomerSequences;
                int index = UnityEngine.Random.Range(0, sequencesInfo.Count);
                sequenceInfo = sequencesInfo[index];
            }
            else if (m_DataLevel.CustomerSequence == CustomerSequenceType.RandomSequence)
            {
                sequenceInfo = new CustomerSequenceInfo();
                sequenceInfo.UseQueueSpawnDelay = true;
                sequenceInfo.SequenceIndexInfo = new List<CustomerSequenceIndexInfo>();

                int count = m_DataLevel.CustomersData.Count;
                List<int> shuffledIndices = GlobalFunctions.GetShuffledRange(0, count - 1);

                // Assign shuffled indices to the sequence
                for (int i = 0; i < count; i++)
                {
                    CustomerData customerData = m_DataLevel.CustomersData[i];

                    CustomerSequenceIndexInfo customerSequenceIndexInfo = new CustomerSequenceIndexInfo();
                    customerSequenceIndexInfo.SequenceIndex = shuffledIndices[i];
                    customerSequenceIndexInfo.QueueSpawnDelay = customerData.QueueSpawnDelay;

                    sequenceInfo.SequenceIndexInfo.Add(customerSequenceIndexInfo);
                }
            }
            else if (m_DataLevel.CustomerSequence == CustomerSequenceType.OrderSwapping)
            {
                customersData = new List<CustomerData>();
                foreach (var customerData in m_DataLevel.CustomersData)
                {
                    customersData.Add(customerData);
                }

                int count = m_DataLevel.CustomersData.Count;
                for (int i = count - 1; i > 0; i--)
                {
                    if (!customersData[i].IsCustomerVIP)
                    {
                        int j = UnityEngine.Random.Range(0, i + 1);
                        if(!customersData[j].IsCustomerVIP)
                        {
                            CustomerData temp = customersData[i];
                            customersData[i] = customersData[j];
                            customersData[j] = temp;
                        }
                    }
                }
            }

            bool shouldContinue = true;

            do
            {
                if (m_DataLevel.CustomersData != null
                    && sequenceInfo != null 
                    && sequenceInfo.SequenceIndexInfo.Count > 0)
                {
                    for (int i = 0; i < sequenceInfo.SequenceIndexInfo.Count; i++)
                    {
                        yield return new WaitWhile(() => s_Instance.m_ShoudGenerateCustomer == false);

                        CustomerSequenceIndexInfo sequenceIndexInfo = sequenceInfo.SequenceIndexInfo[i];
                        CustomerData customerData = m_DataLevel.CustomersData[sequenceIndexInfo.SequenceIndex];

                        yield return new WaitWhile(() => IsQueueFull());

                        if (sequenceInfo.UseQueueSpawnDelay)
                        {
                            yield return new WaitForSeconds(sequenceIndexInfo.QueueSpawnDelay);
                        }
                        else
                        {
                            yield return new WaitForSeconds(customerData.QueueSpawnDelay);
                        }

                        if (hasCusomterConstraint)
                        {
                            if (m_CustomerCounter >= cusomterConstraintValue)
                            {
                                shouldContinue = false;
                                break;
                            }
                        }

                        yield return new WaitWhile(() => s_Instance.m_ShoudGenerateCustomer == false);

                        if (customerData.Customer == CustomerType.Salon)
                        {
                            CustomerSalonController selectedPrefab = customerData.CustomerSalonPrefab;
                            CustomerSalonController customer = Instantiate(selectedPrefab, m_StartNode.transform.position, Quaternion.identity, transform);
                            m_CurrentSalonCustomers.Add(customer);

                            QueueInfo currentQueue = null;
                            if (CustomerPatienceManager.HasSofaPatience())
                            {
                                currentQueue = m_SofaQueue.CurrentCustomer == null ? m_SofaQueue : m_CustomerQueue.Find((x) => x.CurrentCustomer == null);
                            }
                            else
                            {
                                currentQueue = m_CustomerQueue.Find((x) => x.CurrentCustomer == null);
                            }
                            customer.Setup(customerData, m_StartNode, currentQueue, m_CustomerWalkSpeed);
                        }
                        else if(customerData.Customer == CustomerType.Cafe)
                        {
                            CustomerCafeController selectedPrefab = customerData.CustomerCafePrefab;
                            CustomerCafeController customer = Instantiate(selectedPrefab, m_StartNode.transform.position, Quaternion.identity, transform);
                            m_CurrentCafeCustomers.Add(customer);

                            QueueInfo currentQueue = null;
                            if (CustomerPatienceManager.HasSofaPatience())
                            {
                                currentQueue = m_SofaQueue.CurrentCustomer == null ? m_SofaQueue : m_CustomerQueue.Find((x) => x.CurrentCustomer == null);
                            }
                            else
                            {
                                currentQueue = m_CustomerQueue.Find((x) => x.CurrentCustomer == null);
                            }
                            customer.Setup(customerData, m_StartNode, currentQueue, m_CustomerWalkSpeed);
                        }

                        if (hasCusomterConstraint)
                        {
                            m_CustomerCounter++;
                            if (m_CustomerCounter >= cusomterConstraintValue)
                            {
                                shouldContinue = false;
                                break;
                            }
                        }
                    }

                }
                else if(m_DataLevel.CustomersData != null &&
                    m_DataLevel.CustomersData.Count > 0)
                {
                    int index = 0;
                    foreach (var customerData in m_DataLevel.CustomersData)
                    {
                        yield return new WaitWhile(() => s_Instance.m_ShoudGenerateCustomer == false);

                        yield return new WaitWhile(() => IsQueueFull());

                        yield return new WaitForSeconds(customerData.QueueSpawnDelay);

                        if (hasCusomterConstraint)
                        {
                            if (m_CustomerCounter >= cusomterConstraintValue)
                            {
                                shouldContinue = false;
                                break;
                            }
                        }

                        yield return new WaitWhile(() => s_Instance.m_ShoudGenerateCustomer == false);

                        if (customerData.Customer == CustomerType.Salon)
                        {
                            CustomerSalonController selectedPrefab = customerData.CustomerSalonPrefab;
                            CustomerSalonController customer = Instantiate(selectedPrefab, m_StartNode.transform.position, Quaternion.identity, transform);
                            m_CurrentSalonCustomers.Add(customer);

                            QueueInfo currentQueue = null;
                            if (CustomerPatienceManager.HasSofaPatience())
                            {
                                currentQueue = m_SofaQueue.CurrentCustomer == null ? m_SofaQueue : m_CustomerQueue.Find((x) => x.CurrentCustomer == null);
                            }
                            else
                            {
                                currentQueue = m_CustomerQueue.Find((x) => x.CurrentCustomer == null);
                            }

                            if (m_DataLevel.CustomerSequence == CustomerSequenceType.OrderSwapping)
                            {
                                customer.Setup(customersData[index], m_StartNode, currentQueue, m_CustomerWalkSpeed);
                            }
                            else
                            {
                                customer.Setup(customerData, m_StartNode, currentQueue, m_CustomerWalkSpeed);
                            }
                        }
                        else if(customerData.Customer == CustomerType.Cafe)
                        {
                            CustomerCafeController selectedPrefab = customerData.CustomerCafePrefab;
                            CustomerCafeController customer = Instantiate(selectedPrefab, m_StartNode.transform.position, Quaternion.identity, transform);
                            m_CurrentCafeCustomers.Add(customer);

                            QueueInfo currentQueue = null;
                            if (CustomerPatienceManager.HasSofaPatience())
                            {
                                currentQueue = m_SofaQueue.CurrentCustomer == null ? m_SofaQueue : m_CustomerQueue.Find((x) => x.CurrentCustomer == null);
                            }
                            else
                            {
                                currentQueue = m_CustomerQueue.Find((x) => x.CurrentCustomer == null);
                            }

                            if (m_DataLevel.CustomerSequence == CustomerSequenceType.OrderSwapping)
                            {
                                customer.Setup(customersData[index], m_StartNode, currentQueue, m_CustomerWalkSpeed);
                            }
                            else
                            {
                                customer.Setup(customerData, m_StartNode, currentQueue, m_CustomerWalkSpeed);
                            }
                        }

                        if (hasCusomterConstraint)
                        {
                            m_CustomerCounter++;
                            if (m_CustomerCounter >= cusomterConstraintValue)
                            {
                                shouldContinue = false;
                                break;
                            }
                        }

                        index++;
                    }
                }
                else
                {
                    Debug.LogWarning("No Customer Data Found");
                    shouldContinue = false;
                }

            } while (shouldContinue);
        }


        private bool IsQueueFull()
        {
            if (CustomerPatienceManager.HasSofaPatience())
            {
                return m_CustomerQueue.TrueForAll((x) => x.CurrentCustomer != null) && (m_SofaQueue.CurrentCustomer != null);
            }
            else
            {
                return m_CustomerQueue.TrueForAll((x) => x.CurrentCustomer != null);
            }
        }

        public static void ResetQueue()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            List<QueueInfo> adjustedQueue = new List<QueueInfo>();
            if(CustomerPatienceManager.HasSofaPatience())
            {
                adjustedQueue.Add(s_Instance.m_SofaQueue);
            }

            foreach (var queue in s_Instance.m_CustomerQueue)
            {
                adjustedQueue.Add(queue);
            }

            List<QueueInfo> emptyQueueSpots = new List<QueueInfo>();
            foreach (var queue in adjustedQueue)
            {
                if (queue.CurrentCustomer == null)
                {
                    emptyQueueSpots.Add(queue);
                }
                else if(queue.CurrentCustomer != null && emptyQueueSpots.Count > 0)
                {
                    CustomerSalonController salonController = queue.CurrentCustomer as CustomerSalonController;
                    CustomerCafeController cafeController = queue.CurrentCustomer as CustomerCafeController;
                    if (salonController != null &&
                        salonController.IsPicked() == false)
                    {
                        QueueInfo currentQueue = emptyQueueSpots[0];
                        currentQueue.CurrentCustomer = queue.CurrentCustomer;

                        salonController.ResetQueue(currentQueue);

                        queue.CurrentCustomer = null;
                        emptyQueueSpots.Add(queue);

                        emptyQueueSpots.RemoveAt(0);
                    }
                    else if (cafeController != null &&
                        cafeController.IsPicked() == false)
                    {
                        QueueInfo currentQueue = emptyQueueSpots[0];
                        currentQueue.CurrentCustomer = queue.CurrentCustomer;

                        cafeController.ResetQueue(currentQueue);

                        queue.CurrentCustomer = null;
                        emptyQueueSpots.Add(queue);

                        emptyQueueSpots.RemoveAt(0);
                    }
                    else
                    {
                        Debug.LogWarning("Not a CustomerSalonController or CustomerCafeController");
                    }
                }
            }
        }

        public static void RemoveFromQueue(CustomerBaseController customerController)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            CustomerSalonController customerSalon = customerController as CustomerSalonController;
            CustomerCafeController customerCafe = customerController as CustomerCafeController;
            if (customerSalon != null)
            {
                s_Instance.m_CurrentSalonCustomers.Remove(customerSalon);
            }
            else if(customerCafe)
            {
                s_Instance.m_CurrentCafeCustomers.Remove(customerCafe);
            }
        }
        #endregion

        #region Helper
        public static Transform GetTransform()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return null;
            }

            return s_Instance.transform;
        }

        public static PathNode GetExitNode()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return null;
            }

            return s_Instance.m_ExitNode;
        }

        // Returns target customer and its current orders (Worker order no incuded)
        public static List<Tuple<CustomerSalonController, List<Tuple<DataConsumable, Vector3>>>> GetCurrentWaitressOrders()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return null;
            }

            List<Tuple<CustomerSalonController, List<Tuple<DataConsumable, Vector3>>>> customersOrderInfo = new List<Tuple<CustomerSalonController, List<Tuple<DataConsumable, Vector3>>>>();

            foreach (var salonCustomers in s_Instance.m_CurrentSalonCustomers)
            {
                if (salonCustomers != null)
                {
                    List<Tuple<DataConsumable, Vector3>> temordersInfo = salonCustomers.GetCurrentWaitressOrders();

                    if (temordersInfo != null && temordersInfo.Count > 0)
                    {
                        List<Tuple<DataConsumable, Vector3>> ordersInfo = new List<Tuple<DataConsumable, Vector3>>();

                        foreach (var temOrderInfo in temordersInfo)
                        {
                            ordersInfo.Add(temOrderInfo);
                        }

                        Tuple<CustomerSalonController, List<Tuple<DataConsumable, Vector3>>> temCustomerOrderInfo
                            = new Tuple<CustomerSalonController, List<Tuple<DataConsumable, Vector3>>>(salonCustomers, ordersInfo);

                        customersOrderInfo.Add(temCustomerOrderInfo);
                    }
                }
            }

            return customersOrderInfo;
        }


        public static CustomerSalonController GetSalonCustomerInQueue(int atQueueIndex)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return null;
            }

            int adjustedIndex = atQueueIndex;
            if (CustomerPatienceManager.HasSofaPatience())
            {
                if (atQueueIndex == 0)
                {
                    return s_Instance.m_SofaQueue.CurrentCustomer as CustomerSalonController;
                }
                else
                {
                    adjustedIndex = atQueueIndex - 1;
                }
            }

            QueueInfo queueInfo = s_Instance.m_CustomerQueue[adjustedIndex];
            if (queueInfo != null)
            {
                return queueInfo.CurrentCustomer as CustomerSalonController;
            }
            else
            {
                Debug.LogWarning("Salon Queue at index " + atQueueIndex + " not found");
                return null;
            }
        }

        public static List<CustomerSalonController> GetAllSalonCustomer()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return null;
            }

            List<CustomerSalonController> customers = new List<CustomerSalonController>();

            foreach (var customer in s_Instance.m_CurrentSalonCustomers)
            {
                customers.Add(customer);
            }

            return customers;
        }

        public static List<CustomerCafeController> GetAllCafeCustomer()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return null;
            }

            List<CustomerCafeController> customers = new List<CustomerCafeController>();

            foreach (var customer in s_Instance.m_CurrentCafeCustomers)
            {
                customers.Add(customer);
            }

            return customers;
        }

        public static void CustomerGenerationOn()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_ShoudGenerateCustomer = true;
        }

        public static void CustomerGenerationOff()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_ShoudGenerateCustomer = false;
        }


        public static void AddExtraCustomers(int extraCustomers)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_ExtraCustomer += extraCustomers;

            s_Instance.StopCustomerEntering();
            s_Instance.m_CustomerEntering = s_Instance.StartCoroutine(s_Instance.CustomerEntering());
        }

        public static void ResetCustomersForLevelChance()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            foreach (var salonCustomer in s_Instance.m_CurrentSalonCustomers)
            {
                if (salonCustomer != null && salonCustomer.IsAboutToLeaveUnserved())
                {
                    salonCustomer.ResetCustomerWaitOnLevelChance();
                }
            }

            foreach (var cafeCustomer in s_Instance.m_CurrentCafeCustomers)
            {
                if (cafeCustomer != null && cafeCustomer.IsAboutToLeaveUnserved())
                {
                    cafeCustomer.ResetCustomerWaitOnLevelChance();
                }
            }
        }
        #endregion



        #region Global Event
        private void OnTimeFrozeBooster(bool activation)
        {
            IsTimeFrozeBoosterActivated = activation;
        }

        private void OnCustomerWaitFreeze(bool value)
        {
            IsCustomerWaitFrozen = value;
        }


        private void OnGamesEnd()
        {
            m_ShoudGenerateCustomer = false;

            List<QueueInfo> adjustedQueue = new List<QueueInfo>();
            if (CustomerPatienceManager.HasSofaPatience())
            {
                adjustedQueue.Add(s_Instance.m_SofaQueue);
            }

            foreach (var salonQueue in s_Instance.m_CustomerQueue)
            {
                adjustedQueue.Add(salonQueue);
            }

            foreach (var salonQueue in adjustedQueue)
            {
                if (salonQueue.CurrentCustomer != null)
                {
                    CustomerSalonController salonController = salonQueue.CurrentCustomer as CustomerSalonController;
                    CustomerCafeController cafeController = salonQueue.CurrentCustomer as CustomerCafeController;
                    if (salonController != null)
                    {
                        salonController.LeaveImmeditalityForGamesEnd();
                    }
                    else if(cafeController != null)
                    {
                        cafeController.LeaveImmeditalityForGamesEnd();
                    }
                    else
                    {
                        Debug.LogWarning("Not a CustomerSalonController or CustomerCafeController");
                    }
                }
            }
        }
        #endregion

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(CustomerManager) + " is null");

        }
    }

    [Serializable]
    public class QueueInfo
    {
        public PathNode Node;
        [AllowNesting, ReadOnly]
        public CustomerBaseController CurrentCustomer = null;
    }
}
