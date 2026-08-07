using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Isometric.Customer;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;

namespace Isometric.Data
{
    public class HospitalEconomyCustomerImporter : MonoBehaviour
    {
        [Header("Existing DataLevels")]
        [SerializeField] private Object m_OutputFolderAsset;
        [SerializeField] private string m_AssetNamePrefix = "Map1-DataLevel";
        [SerializeField, Min(1)] private int m_MinLevelToUpdate = 1;
        [SerializeField, Min(1)] private int m_MaxLevelToUpdate = 60;

        [Header("Customer Mapping")]
        [SerializeField] private List<LevelCustomerPrefabMapping> m_CustomerPrefabMappings =
            new List<LevelCustomerPrefabMapping>();

        [SerializeField] private int m_CustomerShuffleSeed = 12345;

        [Button("Update Customers")]
        public void UpdateCustomers()
        {
            try
            {
                UpdateCustomersInternal();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to update hospital customers.\n{exception}");
            }
        }

        private void UpdateCustomersInternal()
        {
            string outputFolderPath = GetFolderPath(m_OutputFolderAsset, "Output Folder");

            int minLevel = Mathf.Min(m_MinLevelToUpdate, m_MaxLevelToUpdate);
            int maxLevel = Mathf.Max(m_MinLevelToUpdate, m_MaxLevelToUpdate);

            List<string> existingAssetPaths = new List<string>();
            for (int stageId = minLevel; stageId <= maxLevel; stageId++)
            {
                string assetPath = GetLevelAssetPath(outputFolderPath, stageId);
                if (AssetDatabase.LoadAssetAtPath<DataLevel>(assetPath) != null)
                {
                    existingAssetPaths.Add(assetPath);
                }
            }

            if (existingAssetPaths.Count == 0)
            {
                Debug.LogWarning(
                    $"No existing DataLevel assets were found in {outputFolderPath} " +
                    $"for levels {minLevel} to {maxLevel}.");
                return;
            }

            string preview = string.Join("\n", existingAssetPaths.Take(10));
            if (existingAssetPaths.Count > 10)
            {
                preview += $"\n...and {existingAssetPaths.Count - 10} more";
            }

            bool shouldContinue = EditorUtility.DisplayDialog(
                "Update Hospital Customers",
                "Only customer prefab mappings and new-customer unlock information will be updated.\n" +
                "All other DataLevel values will remain unchanged.\n\n" +
                preview,
                "Continue",
                "Cancel");

            if (!shouldContinue)
            {
                Debug.Log("Hospital customer update cancelled.");
                return;
            }

            int updatedLevels = 0;
            int skippedLevels = 0;

            for (int stageId = minLevel; stageId <= maxLevel; stageId++)
            {
                string assetPath = GetLevelAssetPath(outputFolderPath, stageId);
                DataLevel dataLevel = AssetDatabase.LoadAssetAtPath<DataLevel>(assetPath);

                if (dataLevel == null)
                {
                    skippedLevels++;
                    Debug.LogWarning($"Skipped Level {stageId}: DataLevel asset was not found at {assetPath}.");
                    continue;
                }

                if (!TryUpdateCustomerData(dataLevel, stageId))
                {
                    skippedLevels++;
                    continue;
                }

                EditorUtility.SetDirty(dataLevel);
                AssetDatabase.SaveAssetIfDirty(dataLevel);
                updatedLevels++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Updated customer mappings in {updatedLevels} DataLevel assets." +
                (skippedLevels > 0 ? $" Skipped {skippedLevels} levels; check warnings for details." : string.Empty));
        }

        private bool TryUpdateCustomerData(DataLevel dataLevel, int stageId)
        {
            List<CustomerSalonController> availablePrefabs = GetAvailableCustomerPrefabs(stageId);

            SerializedObject serializedObject = new SerializedObject(dataLevel);
            SerializedProperty customersProperty = serializedObject.FindProperty("m_CustomersData");

            if (customersProperty == null)
            {
                Debug.LogError(
                    $"Skipped Level {stageId}: serialized property 'm_CustomersData' was not found.",
                    dataLevel);
                return false;
            }

            if (customersProperty.arraySize > 0 && availablePrefabs.Count == 0)
            {
                Debug.LogWarning(
                    $"Skipped Level {stageId}: no customer prefabs are available for this level. " +
                    "Existing customer prefab references were left unchanged.",
                    dataLevel);
                return false;
            }

            PrefabShufflePool<CustomerSalonController> prefabPool =
                new PrefabShufflePool<CustomerSalonController>(
                    availablePrefabs,
                    m_CustomerShuffleSeed + (stageId * 1000) + 101);

            for (int customerIndex = 0; customerIndex < customersProperty.arraySize; customerIndex++)
            {
                SerializedProperty customerProperty =
                    customersProperty.GetArrayElementAtIndex(customerIndex);

                SerializedProperty salonPrefabProperty =
                    customerProperty.FindPropertyRelative("CustomerSalonPrefab");

                if (salonPrefabProperty == null)
                {
                    Debug.LogError(
                        $"Skipped Level {stageId}: 'CustomerSalonPrefab' was not found " +
                        $"for customer index {customerIndex}.",
                        dataLevel);
                    return false;
                }

                salonPrefabProperty.objectReferenceValue = prefabPool.Next();
            }

            UpdateNewCustomerUnlockInfo(serializedObject, stageId);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private void UpdateNewCustomerUnlockInfo(SerializedObject serializedObject, int stageId)
        {
            List<CustomerId> newlyUnlockedCustomers = GetNewlyUnlockedCustomerIds(stageId);

            SerializedProperty hasNewCustomerProperty =
                serializedObject.FindProperty("m_HasNewCustomer");

            SerializedProperty newCustomersHolderProperty =
                serializedObject.FindProperty("m_NewCustomersInfo");

            if (hasNewCustomerProperty == null || newCustomersHolderProperty == null)
            {
                throw new InvalidOperationException(
                    "Customer unlock properties were not found on DataLevel.");
            }

            SerializedProperty newCustomersProperty =
                newCustomersHolderProperty.FindPropertyRelative("NewCustomersInfo");

            if (newCustomersProperty == null)
            {
                throw new InvalidOperationException(
                    "'m_NewCustomersInfo.NewCustomersInfo' was not found on DataLevel.");
            }

            hasNewCustomerProperty.boolValue = newlyUnlockedCustomers.Count > 0;
            newCustomersProperty.arraySize = newlyUnlockedCustomers.Count;

            for (int index = 0; index < newlyUnlockedCustomers.Count; index++)
            {
                newCustomersProperty
                    .GetArrayElementAtIndex(index)
                    .enumValueIndex = (int)newlyUnlockedCustomers[index];
            }
        }

        private List<CustomerSalonController> GetAvailableCustomerPrefabs(int stageId)
        {
            return m_CustomerPrefabMappings
                .Where(mapping =>
                    mapping != null &&
                    mapping.SalonPrefab != null &&
                    mapping.UseFromLevel <= stageId)
                .Select(mapping => mapping.SalonPrefab)
                .Distinct()
                .ToList();
        }

        private List<CustomerId> GetNewlyUnlockedCustomerIds(int stageId)
        {
            return m_CustomerPrefabMappings
                .Where(mapping =>
                    mapping != null &&
                    mapping.RegisterAsNewCustomerUnlock &&
                    mapping.UseFromLevel == stageId)
                .Select(mapping => mapping.CustomerId)
                .Distinct()
                .ToList();
        }

        private string GetLevelAssetPath(string outputFolderPath, int stageId)
        {
            return $"{outputFolderPath}/{m_AssetNamePrefix}{stageId}.asset";
        }

        private static string GetFolderPath(Object asset, string fieldName)
        {
            if (asset == null)
            {
                throw new InvalidOperationException($"{fieldName} is not assigned.");
            }

            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrWhiteSpace(assetPath) ||
                !AssetDatabase.IsValidFolder(assetPath))
            {
                throw new InvalidOperationException(
                    $"{fieldName} must reference a project folder.");
            }

            return assetPath;
        }

        private sealed class PrefabShufflePool<T> where T : UnityEngine.Object
        {
            private readonly List<T> m_Source;
            private readonly System.Random m_Random;
            private Queue<T> m_Queue;

            public PrefabShufflePool(List<T> source, int seed)
            {
                m_Source = source ?? new List<T>();
                m_Random = new System.Random(seed);
                m_Queue = new Queue<T>();
            }

            public T Next()
            {
                if (m_Source.Count == 0)
                {
                    return null;
                }

                if (m_Queue.Count == 0)
                {
                    RefillQueue();
                }

                return m_Queue.Dequeue();
            }

            private void RefillQueue()
            {
                List<T> shuffled = new List<T>(m_Source);

                for (int index = shuffled.Count - 1; index > 0; index--)
                {
                    int swapIndex = m_Random.Next(index + 1);

                    T temp = shuffled[index];
                    shuffled[index] = shuffled[swapIndex];
                    shuffled[swapIndex] = temp;
                }

                m_Queue = new Queue<T>(shuffled);
            }
        }

        [Serializable]
        public sealed class LevelCustomerPrefabMapping
        {
            [Min(1)] public int UseFromLevel = 1;
            public bool RegisterAsNewCustomerUnlock = true;
            public CustomerId CustomerId;
            public CustomerSalonController SalonPrefab;
        }
    }
}
#endif
