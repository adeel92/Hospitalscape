using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;
using Isometric.Customer;

namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataLevel", menuName = "GameData/DataLevel")]
    public class DataLevel : ScriptableObject
    {
        [SerializeField] bool m_EditComment;
        [SerializeField, EnableIf(nameof(m_EditComment)), TextArea]
        string m_Comment;

        [Header("---Target---")]
        [SerializeField] LevelGoalInfo m_LevelGoal;
        public LevelGoalInfo LevelGoal => m_LevelGoal;

        [Header("---Key Target---")]
        [SerializeField] int m_Key1TargetValue;
        public int Key1TargetValue => m_Key1TargetValue;

        [SerializeField] int m_Key2TargetValue;
        public int Key2TargetValue => m_Key2TargetValue;


        [Header("---Constraints---")]
        [SerializeField] List<LevelConstraintInfo> m_LevelConstraintInfos;
        public List<LevelConstraintInfo> LevelConstraintInfos => m_LevelConstraintInfos;


        [Header("---Parameter---")]
        [SerializeField] int m_LevelDifficulty;
        public int LevelDifficulty => m_LevelDifficulty;

        [Header("---Customer Unlocking---")]
        [SerializeField] bool m_HasNewCustomer;
        public bool HasNewCustomer => m_HasNewCustomer;

        [SerializeField, ShowIf(nameof(m_HasNewCustomer))]
        CustomerNewInfo m_NewCustomersInfo;
        public CustomerNewInfo NewCustomersInfo => m_NewCustomersInfo;

        [Header("---Customers---")]
        [SerializeField] CustomerSequenceType m_CustomerSalonSequence;
        public CustomerSequenceType CustomerSequence => m_CustomerSalonSequence;

        [Header("-Sequence is going to be selected from the defined sequence")]
        [SerializeField, ShowIf(nameof(m_CustomerSalonSequence), CustomerSequenceType.DefainedSquence)] 
        private CustomerSequencesInfo m_CustomerSalonSequencesInfo;
        public CustomerSequencesInfo CustomerSequencesInfo => m_CustomerSalonSequencesInfo;

        public List<CustomerData> CustomersData => m_CustomersData;
        [SerializeField] List<CustomerData> m_CustomersData;

        [Header("---Sun Rays Activate---")]
        [SerializeField] List<PatienceSunRaysData> m_PatienceSunRaysData;
        public List<PatienceSunRaysData> PatienceSunRaysData => m_PatienceSunRaysData;

    }

    #region Level Goal
    public enum LevelGoalType
    {
        CollectCoins, ServeCustomers
    }

    [Serializable]
    public class LevelGoalInfo
    {
        public LevelGoalType GoalType;

        [AllowNesting, ShowIf(nameof(GoalType), LevelGoalType.CollectCoins)]
        public int NumberCoinsToCollect;
        [AllowNesting, ShowIf(nameof(GoalType), LevelGoalType.ServeCustomers)]
        public int NumberOfCustomersToServe;
    }
    #endregion

    #region Level Constraint
    public enum LevelConstraintType
    {
        TimeConstraint, NumberOfCustomers, DoNotLoseCustomer
    }

    [Serializable]
    public class LevelConstraintInfo
    {
        public LevelConstraintType ConstraintType;

        [AllowNesting, ShowIf(nameof(ConstraintType), LevelConstraintType.TimeConstraint)]
        public int TimeConstraints;
        [AllowNesting, ShowIf(nameof(ConstraintType), LevelConstraintType.NumberOfCustomers)]
        public int NumberOfCustomers;
    }

    #endregion

    #region Customer Data

    public enum CustomerId
    {
        Customer1, Cusomter2, Customer3, Customer4, Customer5, Customer6, Customer7, Customer8, Customer9, Customer10, Customer11
    }

    [Serializable]
    public class CustomerNewInfo
    {
        public List<CustomerId> NewCustomersInfo;
    }

    #region Sequence Related

    public enum CustomerSequenceType
    {
        Default, 
        DefainedSquence,
        //Not shuffling the delay in the Customer Data only the customers
        RandomSequence,
        //Order swapping from customer to customer but Not VIPs
        OrderSwapping,
    }

    [Serializable]
    public class CustomerSequencesInfo
    {
        public List<CustomerSequenceInfo> CustomerSequences;
    }

    [Serializable]
    public class CustomerSequenceInfo
    {
        public bool UseQueueSpawnDelay;
        public List<CustomerSequenceIndexInfo> SequenceIndexInfo;
    }

    [Serializable]
    public class CustomerSequenceIndexInfo
    {
        public int SequenceIndex;
        public float QueueSpawnDelay;
    }
    #endregion

    [Serializable]
    public class CustomerData
    {
        public CustomerType Customer;

        [AllowNesting, ShowIf(nameof(Customer), CustomerType.Salon)]
        public CustomerSalonController CustomerSalonPrefab;

        [AllowNesting, ShowIf(nameof(Customer), CustomerType.Cafe)]
        public CustomerCafeController CustomerCafePrefab;

        public CustomerCommonSetting CustomerCommonSettings;

        public float QueueSpawnDelay;

        public bool IsCustomerVIP;

        public bool IsFirstOrderUndecided;

        [AllowNesting, ShowIf(nameof(Customer), CustomerType.Salon)]
        public CustomerFirstOrderHolder CustomerFirstOrdersInfoHolder;

        public List<CustomerOrderBundleInfo> CustomerOrderBundles;
    }

    [Serializable]
    public class CustomerOrderBundleInfo
    {
        public List<CustomerOrderInfo> CustomerOrdersInfo;
    }

    [Serializable]
    public class CustomerOrderInfo
    {
        public List<DataConsumable> OrdersConsumable;
    }

    [Serializable]
    public class CustomerFirstOrderHolder
    {
        public List<CustomerFirstOrderInfo> CustomerFirstOrdersInfo;
    }

    [Serializable]
    public class CustomerFirstOrderInfo
    {
        public DataConsumable OrderConsumable;
    }
    #endregion

    #region Patience
    [Serializable]
    public class PatienceSunRaysData
    {
        public float ActivationDelay;
    }
    #endregion
}
