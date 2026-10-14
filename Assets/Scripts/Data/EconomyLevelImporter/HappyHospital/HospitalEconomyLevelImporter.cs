using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NaughtyAttributes;
using UnityEngine;
using Object = UnityEngine.Object;
using Isometric.Customer;

#if UNITY_EDITOR
using UnityEditor;

namespace Isometric.Data
{
    public class HospitalEconomyLevelImporter : MonoBehaviour
    {
        [Header("Workbook")]
        [SerializeField] private Object m_WorkbookAsset;
        [SerializeField] private string m_LevelSheetName = "HospitalLevels";
        [SerializeField, Min(1)] private int m_MinLevelToImport = 1;
        [SerializeField, Min(1)] private int m_MaxLevelToImport = 40;

        [Header("Output")]
        [SerializeField] private Object m_OutputFolderAsset;
        [SerializeField] private string m_AssetNamePrefix = "Map1-DataLevel-";

        [Header("Defaults")]
        [SerializeField] private List<LevelCustomerPrefabMapping> m_CustomerPrefabMappings = new List<LevelCustomerPrefabMapping>();
        [SerializeField] private CustomerCommonSetting m_DefaultSalonCommonSetting;
        [SerializeField] private CustomerSequenceType m_DefaultCustomerSequence = CustomerSequenceType.Default;
        [SerializeField] private int m_DefaultKey1TargetValue;
        [SerializeField] private int m_DefaultKey2TargetValue;

        [Header("Spawn Delay")]
        [SerializeField] private float m_CustomerQueueSpawnMinDelay = 4;
        [SerializeField] private float m_CustomerQueueSpawnMaxDelay = 8;

        [Header("Difficulty Mapping")]
        [SerializeField] private List<DifficultyMapping> m_DifficultyMappings = new List<DifficultyMapping>();

        [Header("First Order Mapping")]
        [SerializeField] private List<FirstOrderMapping> m_FirstOrderMappings = new List<FirstOrderMapping>();

        [Header("Follow-up Order Mapping")]
        [SerializeField] private List<OrderMapping> m_OrderMappings = new List<OrderMapping>();

        [Header("Order Bundling")]
        [SerializeField] private bool m_EnableRandomOrderBundling;
        [SerializeField, Min(1)] private int m_MinOrdersPerBundle = 1;
        [SerializeField, Min(1)] private int m_MaxOrdersPerBundle = 3;
        [SerializeField] private int m_OrderBundlingSeed = 12345;
        [SerializeField] private List<DataConsumable> m_OrdersThatCannotBeBundled = new List<DataConsumable>();

        [Button("Generate DataLevels")]
        public void GenerateDataLevels()
        {
            try
            {
                ImportInternal();
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to import economy levels.\n{exception}");
            }
        }

        private void ImportInternal()
        {
            string workbookPath = GetAssetPath(m_WorkbookAsset, "Workbook");
            string outputFolderPath = GetFolderPath(m_OutputFolderAsset, "Output Folder");

            Dictionary<string, string> missingMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            List<WorkbookRow> rows = WorkbookUtility.ReadRows(workbookPath, m_LevelSheetName);
            List<string> existingAssetPaths = GetExistingLevelAssetPaths(rows, outputFolderPath);

            if (existingAssetPaths.Count > 0)
            {
                string preview = string.Join("\n", existingAssetPaths.Take(10));
                if (existingAssetPaths.Count > 10)
                {
                    preview += $"\n...and {existingAssetPaths.Count - 10} more";
                }

                bool shouldContinue = EditorUtility.DisplayDialog(
                    "Existing DataLevel Assets Found",
                    "Some target DataLevel assets already exist and will be overwritten.\n\n" + preview,
                    "Continue",
                    "Cancel");

                if (!shouldContinue)
                {
                    Debug.Log("Level import cancelled because existing DataLevel assets were found.");
                    return;
                }
            }

            int importedLevels = 0;
            foreach (WorkbookRow row in rows)
            {
                string levelValue = row.Get("LEVEL");

                if (!TryParseInt(levelValue, out int stageId))
                {
                    Debug.LogWarning($"ADEEL 1 - LEVEL value: '{levelValue ?? "NULL"}'");
                    continue;
                }

                int minLevel = Mathf.Min(m_MinLevelToImport, m_MaxLevelToImport);
                int maxLevel = Mathf.Max(m_MinLevelToImport, m_MaxLevelToImport);
                if (stageId < minLevel || stageId > maxLevel)
                {
                    Debug.LogWarning($"ADEEL 2 - stageId: {stageId}");
                    continue;
                }

                DataLevel dataLevel = LoadOrCreateLevelAsset(outputFolderPath, stageId);
                PopulateLevel(dataLevel, row, stageId, missingMappings);
                EditorUtility.SetDirty(dataLevel);
                AssetDatabase.SaveAssetIfDirty(dataLevel);
                importedLevels++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (missingMappings.Count > 0)
            {
                string message = string.Join("\n", missingMappings.Select(pair => $"{pair.Key} -> {pair.Value}"));
                Debug.LogWarning($"Imported {importedLevels} levels, but some workbook values are not mapped:\n{message}");
            }
            else
            {
                Debug.Log($"Imported {importedLevels} levels from {Path.GetFileName(workbookPath)} into {outputFolderPath}.");
            }
        }

        private DataLevel LoadOrCreateLevelAsset(string outputFolderPath, int stageId)
        {
            string assetPath = $"{outputFolderPath}/{m_AssetNamePrefix}{stageId}.asset";
            DataLevel dataLevel = AssetDatabase.LoadAssetAtPath<DataLevel>(assetPath);
            if (dataLevel == null)
            {
                dataLevel = ScriptableObject.CreateInstance<DataLevel>();
                AssetDatabase.CreateAsset(dataLevel, assetPath);
            }

            dataLevel.name = $"{m_AssetNamePrefix}{stageId}";
            return dataLevel;
        }

        private List<string> GetExistingLevelAssetPaths(List<WorkbookRow> rows, string outputFolderPath)
        {
            List<string> existingAssetPaths = new List<string>();
            int minLevel = Mathf.Min(m_MinLevelToImport, m_MaxLevelToImport);
            int maxLevel = Mathf.Max(m_MinLevelToImport, m_MaxLevelToImport);

            foreach (WorkbookRow row in rows)
            {
                if (!TryParseInt(row.Get("LEVEL"), out int stageId))
                {
                    continue;
                }

                if (stageId < minLevel || stageId > maxLevel)
                {
                    continue;
                }

                string assetPath = $"{outputFolderPath}/{m_AssetNamePrefix}{stageId}.asset";
                if (AssetDatabase.LoadAssetAtPath<DataLevel>(assetPath) != null)
                {
                    existingAssetPaths.Add(assetPath);
                }
            }

            return existingAssetPaths;
        }

        private void PopulateLevel(
            DataLevel dataLevel,
            WorkbookRow row,
            int stageId,
            IDictionary<string, string> missingMappings)
        {
            SerializedObject serializedObject = new SerializedObject(dataLevel);
            PrefabShufflePool<CustomerSalonController> salonPrefabPool = CreateSalonPrefabPool(stageId);
            List<CustomerId> newlyUnlockedCustomers = GetNewlyUnlockedCustomerIds(stageId);

            SetLevelGoal(serializedObject, row);
            SetLevelConstraints(serializedObject, row);
            SetLevelDifficulty(serializedObject, stageId);

            serializedObject.FindProperty("m_Key1TargetValue").intValue =
                ParseIntOrDefault(row.Get("KEY 1 VALUE"), m_DefaultKey1TargetValue);
            serializedObject.FindProperty("m_Key2TargetValue").intValue =
                ParseIntOrDefault(row.Get("KEY 2 VALUE"), m_DefaultKey2TargetValue);
            serializedObject.FindProperty("m_HasNewCustomer").boolValue = newlyUnlockedCustomers.Count > 0;
            serializedObject.FindProperty("m_CustomerSalonSequence").enumValueIndex = (int)m_DefaultCustomerSequence;
            SerializedProperty newCustomersProperty = serializedObject.FindProperty("m_NewCustomersInfo").FindPropertyRelative("NewCustomersInfo");
            newCustomersProperty.arraySize = newlyUnlockedCustomers.Count;
            for (int index = 0; index < newlyUnlockedCustomers.Count; index++)
            {
                newCustomersProperty.GetArrayElementAtIndex(index).enumValueIndex = (int)newlyUnlockedCustomers[index];
            }
            serializedObject.FindProperty("m_CustomerSalonSequencesInfo").FindPropertyRelative("CustomerSequences").arraySize = 0;
            serializedObject.FindProperty("m_PatienceWeatherData").arraySize = 0;
            serializedObject.FindProperty("m_Comment").stringValue = $"Imported from workbook stage {stageId}";

            SerializedProperty customersProperty = serializedObject.FindProperty("m_CustomersData");
            customersProperty.arraySize = 0;

            int customerWriteIndex = 0;
            for (int customerIndex = 1; customerIndex <= 31; customerIndex++)
            {
                ImportedCustomer importedCustomer = ParseCustomer(
                    row,
                    customerIndex,
                    stageId,
                    salonPrefabPool,
                    missingMappings);
                if (importedCustomer == null)
                {
                    continue;
                }

                bool isFirstCustomer = customerIndex == 1;
                customersProperty.InsertArrayElementAtIndex(customerWriteIndex);
                SerializedProperty customerProperty = customersProperty.GetArrayElementAtIndex(customerWriteIndex);
                WriteCustomer(customerProperty, importedCustomer, isFirstCustomer);
                customerWriteIndex++;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void SetLevelGoal(SerializedObject serializedObject, WorkbookRow row)
        {
            SerializedProperty levelGoalProperty = serializedObject.FindProperty("m_LevelGoal");

            int coinGoal = ParseIntOrDefault(row.Get("LEVEL GOAL(COINS)"));
            int customerGoal = ParseIntOrDefault(row.Get("LEVEL GOAL(PATIENTS)"));

            if (customerGoal > 0)
            {
                levelGoalProperty.FindPropertyRelative("GoalType").enumValueIndex = (int)LevelGoalType.ServeCustomers;
                levelGoalProperty.FindPropertyRelative("NumberOfCustomersToServe").intValue = customerGoal;
                levelGoalProperty.FindPropertyRelative("NumberCoinsToCollect").intValue = 0;
            }
            else
            {
                levelGoalProperty.FindPropertyRelative("GoalType").enumValueIndex = (int)LevelGoalType.CollectCoins;
                levelGoalProperty.FindPropertyRelative("NumberCoinsToCollect").intValue = coinGoal;
                levelGoalProperty.FindPropertyRelative("NumberOfCustomersToServe").intValue = 0;
            }
        }

        private void SetLevelConstraints(SerializedObject serializedObject, WorkbookRow row)
        {
            SerializedProperty constraintsProperty = serializedObject.FindProperty("m_LevelConstraintInfos");
            constraintsProperty.arraySize = 0;

            int timeLimit = ParseIntOrDefault(row.Get("TIME LIMIT CONSTRAINT"));
            int customerLimit = ParseIntOrDefault(row.Get("PATIENT LIMIT CONSTRAINT"));
            bool noLoseCustomers = ParseBool(row.Get("DON'T LOSE PATIENT CONSTRAINT"));

            int writeIndex = 0;

            if (timeLimit > 0)
            {
                constraintsProperty.InsertArrayElementAtIndex(writeIndex);
                SerializedProperty constraintProperty = constraintsProperty.GetArrayElementAtIndex(writeIndex);
                constraintProperty.FindPropertyRelative("ConstraintType").enumValueIndex = (int)LevelConstraintType.TimeConstraint;
                constraintProperty.FindPropertyRelative("TimeConstraints").intValue = timeLimit;
                constraintProperty.FindPropertyRelative("NumberOfCustomers").intValue = 0;
                writeIndex++;
            }

            if (customerLimit > 0)
            {
                constraintsProperty.InsertArrayElementAtIndex(writeIndex);
                SerializedProperty constraintProperty = constraintsProperty.GetArrayElementAtIndex(writeIndex);
                constraintProperty.FindPropertyRelative("ConstraintType").enumValueIndex = (int)LevelConstraintType.NumberOfCustomers;
                constraintProperty.FindPropertyRelative("TimeConstraints").intValue = 0;
                constraintProperty.FindPropertyRelative("NumberOfCustomers").intValue = customerLimit;
                writeIndex++;
            }

            if (noLoseCustomers)
            {
                constraintsProperty.InsertArrayElementAtIndex(writeIndex);
                SerializedProperty constraintProperty = constraintsProperty.GetArrayElementAtIndex(writeIndex);
                constraintProperty.FindPropertyRelative("ConstraintType").enumValueIndex = (int)LevelConstraintType.DoNotLoseCustomer;
                constraintProperty.FindPropertyRelative("TimeConstraints").intValue = 0;
                constraintProperty.FindPropertyRelative("NumberOfCustomers").intValue = 0;
            }
        }

        private void SetLevelDifficulty(SerializedObject serializedObject, int stageId)
        {
            DifficultyMapping mapping = m_DifficultyMappings.Find(item =>
                item != null &&
                item.UseFromLevel <= stageId &&
                stageId <= item.UseToLevel);

            serializedObject.FindProperty("m_LevelDifficulty").intValue =
                mapping != null ? mapping.Value : stageId;
        }

        private ImportedCustomer ParseCustomer(
            WorkbookRow row,
            int customerIndex,
            int stageId,
            PrefabShufflePool<CustomerSalonController> salonPrefabPool,
            IDictionary<string, string> missingMappings)
        {
            string firstOrderLabel = row.Get($"CUSTOMER {customerIndex} DOCTOR COLOR");
            string ordersValue = row.Get($"CUSTOMER {customerIndex} ORDERS");
            string undecidedValue = row.Get($"CUSTOMER {customerIndex} USE RECEPTION");
            string receptionOrdersValue = row.Get($"CUSTOMER {customerIndex} RECEPTION ORDER");

            // mrcHefF
            bool hasAnyData =
                !string.IsNullOrWhiteSpace(firstOrderLabel) ||
                !string.IsNullOrWhiteSpace(ordersValue) ||
                !string.IsNullOrWhiteSpace(undecidedValue) ||
                !string.IsNullOrWhiteSpace(receptionOrdersValue);

            if (!hasAnyData)
            {
                return null;
            }

            FirstOrderMapping firstOrderMapping = FindFirstOrderMapping(firstOrderLabel);
            if (firstOrderMapping == null)
            {
                missingMappings[$"Missing first-order mapping: {firstOrderLabel}"] = $"customer_{customerIndex}_first_order";
                return null;
            }

            ImportedCustomer importedCustomer = new ImportedCustomer
            {
                IsFirstOrderUndecided = undecidedValue.Equals("yes", StringComparison.OrdinalIgnoreCase),
                CustomerCommonSetting = m_DefaultSalonCommonSetting,
                SalonPrefab = salonPrefabPool.Next(),
                FirstOrder = firstOrderMapping.FirstOrderConsumable
            };

            List<DataConsumable> followUpOrders = ParseOrderConsumables(
                ordersValue,
                $"CUSTOMER {customerIndex} ORDERS",
                "follow-up",
                missingMappings);

            importedCustomer.FollowUpOrderGroups = BuildOrderGroups(followUpOrders, stageId, customerIndex);
            importedCustomer.CounterOrders = ParseOrderConsumables(
                receptionOrdersValue,
                $"CUSTOMER {customerIndex} RECEPTION ORDER",
                "reception",
                missingMappings);

            return importedCustomer;
        }

        private void WriteCustomer(SerializedProperty customerProperty, ImportedCustomer customer, bool isFirstCustomer)
        {
            customerProperty.FindPropertyRelative("Customer").enumValueIndex = (int)CustomerType.Salon;
            customerProperty.FindPropertyRelative("CustomerSalonPrefab").objectReferenceValue = customer.SalonPrefab;
            customerProperty.FindPropertyRelative("CustomerCommonSettings").objectReferenceValue = customer.CustomerCommonSetting;
            customerProperty.FindPropertyRelative("QueueSpawnDelay").floatValue = isFirstCustomer ? 0f : Mathf.Round(UnityEngine.Random.Range(m_CustomerQueueSpawnMinDelay, m_CustomerQueueSpawnMaxDelay) * 10f) / 10f;
            customerProperty.FindPropertyRelative("IsCustomerVIP").boolValue = false;
            customerProperty.FindPropertyRelative("IsFirstOrderUndecided").boolValue = customer.IsFirstOrderUndecided;

            SerializedProperty firstOrdersProperty = customerProperty
                .FindPropertyRelative("CustomerFirstOrdersInfoHolder")
                .FindPropertyRelative("CustomerFirstOrdersInfo");

            if (customer.FirstOrder != null)
            {
                firstOrdersProperty.arraySize = 1;
                firstOrdersProperty.GetArrayElementAtIndex(0)
                    .FindPropertyRelative("OrderConsumable")
                    .objectReferenceValue = customer.FirstOrder;
            }
            else
            {
                firstOrdersProperty.arraySize = 0;
            }

            SerializedProperty counterOrdersConsumableProperty = customerProperty
                .FindPropertyRelative("CustomerCounterOrderInfo")
                .FindPropertyRelative("OrdersConsumable");

            counterOrdersConsumableProperty.arraySize = customer.CounterOrders.Count;
            for (int orderIndex = 0; orderIndex < customer.CounterOrders.Count; orderIndex++)
            {
                counterOrdersConsumableProperty.GetArrayElementAtIndex(orderIndex).objectReferenceValue =
                    customer.CounterOrders[orderIndex];
            }

            SerializedProperty bundlesProperty = customerProperty.FindPropertyRelative("CustomerOrderBundles");
            if (customer.FollowUpOrderGroups.Count > 0)
            {
                bundlesProperty.arraySize = 1;
                SerializedProperty bundleProperty = bundlesProperty.GetArrayElementAtIndex(0);
                SerializedProperty ordersInfoProperty = bundleProperty.FindPropertyRelative("CustomerOrdersInfo");
                ordersInfoProperty.arraySize = customer.FollowUpOrderGroups.Count;

                for (int index = 0; index < customer.FollowUpOrderGroups.Count; index++)
                {
                    SerializedProperty orderInfoProperty = ordersInfoProperty.GetArrayElementAtIndex(index);
                    SerializedProperty ordersConsumableProperty = orderInfoProperty.FindPropertyRelative("OrdersConsumable");
                    List<DataConsumable> orderGroup = customer.FollowUpOrderGroups[index];
                    ordersConsumableProperty.arraySize = orderGroup.Count;
                    for (int orderIndex = 0; orderIndex < orderGroup.Count; orderIndex++)
                    {
                        ordersConsumableProperty.GetArrayElementAtIndex(orderIndex).objectReferenceValue = orderGroup[orderIndex];
                    }
                }
            }
            else
            {
                bundlesProperty.arraySize = 0;
            }
        }

        private List<DataConsumable> ParseOrderConsumables(
            string ordersValue,
            string columnName,
            string orderKind,
            IDictionary<string, string> missingMappings)
        {
            List<DataConsumable> orders = new List<DataConsumable>();
            foreach (string orderToken in SplitCommaSeparatedValues(ordersValue))
            {
                if (IsEmptyOrderToken(orderToken))
                {
                    continue;
                }

                OrderMapping orderMapping = FindOrderMapping(orderToken);
                if (orderMapping == null || orderMapping.Consumable == null)
                {
                    missingMappings[$"Missing {orderKind} mapping: {orderToken}"] = columnName;
                    continue;
                }

                orders.Add(orderMapping.Consumable);
            }

            return orders;
        }

        private FirstOrderMapping FindFirstOrderMapping(string label)
        {
            return m_FirstOrderMappings.Find(item =>
                string.Equals(NormalizeMappingLabel(item.Label), NormalizeMappingLabel(label), StringComparison.OrdinalIgnoreCase));
        }

        private OrderMapping FindOrderMapping(string label)
        {
            return m_OrderMappings.Find(item =>
                string.Equals(NormalizeMappingLabel(item.Label), NormalizeMappingLabel(label), StringComparison.OrdinalIgnoreCase));
        }

        private PrefabShufflePool<CustomerSalonController> CreateSalonPrefabPool(int stageId)
        {
            List<CustomerSalonController> prefabs = m_CustomerPrefabMappings
                .Where(item => item != null && item.SalonPrefab != null && item.UseFromLevel <= stageId)
                .Select(item => item.SalonPrefab)
                .Distinct()
                .ToList();

            return new PrefabShufflePool<CustomerSalonController>(prefabs, m_OrderBundlingSeed + (stageId * 1000) + 101);
        }

        private List<CustomerId> GetNewlyUnlockedCustomerIds(int stageId)
        {
            return m_CustomerPrefabMappings
                .Where(item =>
                    item != null &&
                    item.RegisterAsNewCustomerUnlock &&
                    item.UseFromLevel == stageId)
                .Select(item => item.CustomerId)
                .Distinct()
                .ToList();
        }

        private List<List<DataConsumable>> BuildOrderGroups(List<DataConsumable> followUpOrders, int stageId, int customerIndex)
        {
            List<List<DataConsumable>> groups = new List<List<DataConsumable>>();
            if (followUpOrders == null || followUpOrders.Count == 0)
            {
                return groups;
            }

            if (!m_EnableRandomOrderBundling)
            {
                foreach (DataConsumable order in followUpOrders)
                {
                    groups.Add(new List<DataConsumable> { order });
                }

                return groups;
            }

            int minBundleSize = Mathf.Clamp(m_MinOrdersPerBundle, 1, 3);
            int maxBundleSize = Mathf.Clamp(m_MaxOrdersPerBundle, minBundleSize, 3);
            System.Random random = new System.Random(m_OrderBundlingSeed + (stageId * 100) + customerIndex);

            int index = 0;
            while (index < followUpOrders.Count)
            {
                DataConsumable currentOrder = followUpOrders[index];
                if (CannotBeBundled(currentOrder))
                {
                    groups.Add(new List<DataConsumable> { currentOrder });
                    index++;
                    continue;
                }

                int availableBundleableOrders = 0;
                for (int lookAhead = index; lookAhead < followUpOrders.Count; lookAhead++)
                {
                    if (CannotBeBundled(followUpOrders[lookAhead]))
                    {
                        break;
                    }

                    availableBundleableOrders++;
                }

                int randomBundleSize = random.Next(minBundleSize, maxBundleSize + 1);
                int bundleSize = Mathf.Clamp(randomBundleSize, 1, availableBundleableOrders);

                List<DataConsumable> group = new List<DataConsumable>();
                for (int takeIndex = 0; takeIndex < bundleSize; takeIndex++)
                {
                    group.Add(followUpOrders[index + takeIndex]);
                }

                groups.Add(group);
                index += bundleSize;
            }

            return groups;
        }

        private bool CannotBeBundled(DataConsumable consumable)
        {
            return consumable != null && m_OrdersThatCannotBeBundled.Contains(consumable);
        }

        private static IEnumerable<string> SplitCommaSeparatedValues(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                yield break;
            }

            string[] values = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in values)
            {
                string trimmed = item.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    yield return trimmed;
                }
            }
        }

        private static bool IsEmptyOrderToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            string trimmed = value.Trim();
            return trimmed.All(character => character == '.');
        }

        private static string NormalizeMappingLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return string.Empty;
            }

            return string.Join(" ", label.Trim().Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static string GetAssetPath(Object asset, string fieldName)
        {
            if (asset == null)
            {
                throw new InvalidOperationException($"{fieldName} is not assigned.");
            }

            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrWhiteSpace(assetPath) || !File.Exists(assetPath))
            {
                throw new InvalidOperationException($"{fieldName} must reference a file asset.");
            }

            return assetPath;
        }

        private static string GetFolderPath(Object asset, string fieldName)
        {
            if (asset == null)
            {
                throw new InvalidOperationException($"{fieldName} is not assigned.");
            }

            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrWhiteSpace(assetPath) || !AssetDatabase.IsValidFolder(assetPath))
            {
                throw new InvalidOperationException($"{fieldName} must reference a project folder.");
            }

            return assetPath;
        }

        private static bool TryParseInt(string value, out int parsedValue)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedValue))
            {
                return true;
            }

            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
            {
                parsedValue = Mathf.RoundToInt(floatValue);
                return true;
            }

            parsedValue = 0;
            return false;
        }

        private static int ParseIntOrDefault(string value, int defaultValue = 0)
        {
            return TryParseInt(value, out int parsedValue) ? parsedValue : defaultValue;
        }

        private static float ParseFloatOrDefault(string value, float defaultValue = 0f)
        {
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedValue))
            {
                return parsedValue;
            }

            return defaultValue;
        }

        private static bool ParseBool(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (bool.TryParse(value, out bool boolValue))
            {
                return boolValue;
            }

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
            {
                return intValue != 0;
            }

            return string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
        }

        [Serializable]
        private sealed class ImportedCustomer
        {
            public CustomerSalonController SalonPrefab;
            public CustomerCommonSetting CustomerCommonSetting;
            public bool IsFirstOrderUndecided;
            public DataConsumable FirstOrder;
            public List<DataConsumable> CounterOrders = new List<DataConsumable>();
            public List<List<DataConsumable>> FollowUpOrderGroups = new List<List<DataConsumable>>();
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
        public sealed class DifficultyMapping
        {
            [Min(1)] public int UseFromLevel = 1;
            [Min(1)] public int UseToLevel = 1;
            public int Value;
        }

        [Serializable]
        public sealed class LevelCustomerPrefabMapping
        {
            [Min(1)] public int UseFromLevel = 1;
            public bool RegisterAsNewCustomerUnlock = true;
            public CustomerId CustomerId;
            public CustomerSalonController SalonPrefab;
        }

        [Serializable]
        public sealed class FirstOrderMapping
        {
            public string Label;
            public DataConsumable FirstOrderConsumable;
        }

        [Serializable]
        public sealed class OrderMapping
        {
            public string Label;
            public DataConsumable Consumable;
        }

        private sealed class WorkbookRow
        {
            private readonly Dictionary<string, string> m_Values;

            public WorkbookRow(Dictionary<string, string> values)
            {
                m_Values = values;
            }

            public string Get(string key)
            {
                return m_Values.TryGetValue(key, out string value) ? value : string.Empty;
            }
        }

        private static class WorkbookUtility
        {
            private static readonly XNamespace s_SpreadsheetNamespace =
                "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

            private static readonly XNamespace s_RelationshipNamespace =
                "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

            private static readonly XNamespace s_PackageRelationshipNamespace =
                "http://schemas.openxmlformats.org/package/2006/relationships";

            public static List<WorkbookRow> ReadRows(string workbookPath, string sheetName)
            {
                using ZipArchive archive = ZipFile.OpenRead(workbookPath);

                List<string> sharedStrings = LoadSharedStrings(archive);
                string worksheetPath = GetWorksheetPath(archive, sheetName);
                XDocument worksheetDocument = LoadXml(archive, worksheetPath);

                List<Dictionary<string, string>> rows = ParseWorksheetRows(worksheetDocument, sharedStrings);
                if (rows.Count == 0)
                {
                    return new List<WorkbookRow>();
                }

                int headerRowIndex = FindHeaderRowIndex(rows);
                if (headerRowIndex < 0)
                {
                    throw new InvalidOperationException(
                        "Could not find a level header row. Expected headers such as 'stage_id' or 'LEVEL'.");
                }

                Dictionary<int, string> headerMap = new Dictionary<int, string>();
                foreach (KeyValuePair<string, string> headerCell in rows[headerRowIndex])
                {
                    string normalizedHeader = NormalizeHeader(headerCell.Value);
                    if (!string.IsNullOrWhiteSpace(normalizedHeader))
                    {
                        headerMap[ColumnNameToIndex(headerCell.Key)] = normalizedHeader;
                    }
                }

                List<WorkbookRow> workbookRows = new List<WorkbookRow>();
                for (int rowIndex = headerRowIndex + 1; rowIndex < rows.Count; rowIndex++)
                {
                    Dictionary<string, string> rowData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<string, string> cell in rows[rowIndex])
                    {
                        int columnIndex = ColumnNameToIndex(cell.Key);
                        if (headerMap.TryGetValue(columnIndex, out string header))
                        {
                            rowData[header] = cell.Value;
                        }
                    }

                    workbookRows.Add(new WorkbookRow(rowData));
                }

                return workbookRows;
            }

            private static int FindHeaderRowIndex(List<Dictionary<string, string>> rows)
            {
                for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    HashSet<string> normalizedHeaders = new HashSet<string>(
                        rows[rowIndex]
                            .Values
                            .Select(NormalizeHeader)
                            .Where(header => !string.IsNullOrWhiteSpace(header)),
                        StringComparer.OrdinalIgnoreCase);

                    if (normalizedHeaders.Contains("LEVEL") &&
                        (normalizedHeaders.Contains("LEVEL GOAL(COINS)") ||
                         normalizedHeaders.Contains("LEVEL GOAL(PATIENTS)") ||
                         normalizedHeaders.Contains("CUSTOMER 1 ORDERS")))
                    {
                        return rowIndex;
                    }
                }

                return -1;
            }

            private static string NormalizeHeader(string header)
            {
                if (string.IsNullOrWhiteSpace(header))
                {
                    return string.Empty;
                }

                string trimmedHeader = header.Trim();
                string compactHeader = Regex.Replace(trimmedHeader.ToLowerInvariant(), @"[^a-z0-9]+", " ").Trim();
                compactHeader = Regex.Replace(compactHeader, @"\s+", " ");

                switch (compactHeader)
                {
                    case "stage id":
                    case "level":
                        return "LEVEL";
                    case "no of patients":
                    case "no of customers":
                    case "customer count":
                    case "patient count":
                        return "customer_count";
                    case "level goal patients":
                    case "level goal customers":
                    case "goal customers target":
                        return "LEVEL GOAL(PATIENTS)";
                    case "level goal coins":
                    case "goal coins":
                        return "LEVEL GOAL(COINS)";
                    case "key 1 value":
                    case "key1 target value":
                    case "key 1 target value":
                        return "KEY 1 VALUE";
                    case "key 2 value":
                    case "key2 target value":
                    case "key 2 target value":
                        return "KEY 2 VALUE";
                    case "time limit constraint":
                    case "goal time seconds":
                        return "TIME LIMIT CONSTRAINT";
                    case "patient limit constraint":
                    case "customer limit constraint":
                    case "goal customers limit":
                        return "PATIENT LIMIT CONSTRAINT";
                    case "don t lose patient constraint":
                    case "dont lose patient constraint":
                    case "do not lose patient constraint":
                    case "don t lose customer constraint":
                    case "dont lose customer constraint":
                    case "do not lose customer constraint":
                    case "goal no lose customers":
                        return "DON'T LOSE PATIENT CONSTRAINT";
                }

                Match customerMatch = Regex.Match(
                    compactHeader,
                    @"^customer\s+(\d+)\s+(.+)$",
                    RegexOptions.IgnoreCase);
                if (customerMatch.Success)
                {
                    string customerIndex = customerMatch.Groups[1].Value;
                    string customerField = customerMatch.Groups[2].Value;
                    switch (customerField)
                    {
                        case "orders":
                            return $"CUSTOMER {customerIndex} ORDERS";
                        case "doctor color":
                        case "doctor colour":
                        case "first order":
                            return $"CUSTOMER {customerIndex} DOCTOR COLOR";
                        case "use reception":
                            return $"CUSTOMER {customerIndex} USE RECEPTION";
                        case "reception order":
                        case "reception orders":
                        case "counter order":
                        case "counter orders":
                            return $"CUSTOMER {customerIndex} RECEPTION ORDER";
                        case "status":
                            return $"CUSTOMER {customerIndex} STATUS";
                        case "spawn time":
                            return $"customer_{customerIndex}_spawn_time";
                    }
                }

                return Regex.Replace(compactHeader, @"\s+", "_");
            }

            private static string GetWorksheetPath(ZipArchive archive, string sheetName)
            {
                XDocument workbookDocument = LoadXml(archive, "xl/workbook.xml");
                XDocument workbookRelationshipsDocument = LoadXml(archive, "xl/_rels/workbook.xml.rels");

                XElement sheetElement = workbookDocument
                    .Descendants(s_SpreadsheetNamespace + "sheet")
                    .FirstOrDefault(sheet => string.Equals(
                        (string)sheet.Attribute("name"),
                        sheetName,
                        StringComparison.OrdinalIgnoreCase));

                if (sheetElement == null)
                {
                    throw new InvalidOperationException($"Sheet '{sheetName}' was not found in workbook.");
                }

                string relationshipId = (string)sheetElement.Attribute(s_RelationshipNamespace + "id");
                XElement relationshipElement = workbookRelationshipsDocument
                    .Descendants(s_PackageRelationshipNamespace + "Relationship")
                    .FirstOrDefault(element => string.Equals(
                        (string)element.Attribute("Id"),
                        relationshipId,
                        StringComparison.Ordinal));

                if (relationshipElement == null)
                {
                    throw new InvalidOperationException($"Relationship '{relationshipId}' was not found for sheet '{sheetName}'.");
                }

                string target = (string)relationshipElement.Attribute("Target");
                if (string.IsNullOrWhiteSpace(target))
                {
                    throw new InvalidOperationException($"Sheet '{sheetName}' does not have a worksheet target.");
                }

                target = target.Replace('\\', '/');
                if (!target.StartsWith("xl/", StringComparison.OrdinalIgnoreCase))
                {
                    target = $"xl/{target.TrimStart('/')}";
                }

                return target;
            }

            private static List<string> LoadSharedStrings(ZipArchive archive)
            {
                ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");
                if (entry == null)
                {
                    return new List<string>();
                }

                XDocument document = LoadXml(archive, "xl/sharedStrings.xml");
                return document
                    .Descendants(s_SpreadsheetNamespace + "si")
                    .Select(element => string.Concat(element.Descendants(s_SpreadsheetNamespace + "t").Select(t => (string)t)))
                    .ToList();
            }

            private static List<Dictionary<string, string>> ParseWorksheetRows(XDocument worksheetDocument, IReadOnlyList<string> sharedStrings)
            {
                List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();

                foreach (XElement rowElement in worksheetDocument.Descendants(s_SpreadsheetNamespace + "row"))
                {
                    Dictionary<string, string> row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (XElement cellElement in rowElement.Elements(s_SpreadsheetNamespace + "c"))
                    {
                        string reference = (string)cellElement.Attribute("r");
                        if (string.IsNullOrWhiteSpace(reference))
                        {
                            continue;
                        }

                        string columnName = new string(reference.TakeWhile(char.IsLetter).ToArray());
                        row[columnName] = GetCellValue(cellElement, sharedStrings);
                    }

                    rows.Add(row);
                }

                return rows;
            }

            private static string GetCellValue(XElement cellElement, IReadOnlyList<string> sharedStrings)
            {
                string cellType = (string)cellElement.Attribute("t");
                string rawValue = cellElement.Element(s_SpreadsheetNamespace + "v")?.Value ?? string.Empty;

                if (string.Equals(cellType, "s", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int sharedStringIndex)
                        && sharedStringIndex >= 0
                        && sharedStringIndex < sharedStrings.Count)
                    {
                        return sharedStrings[sharedStringIndex];
                    }

                    return string.Empty;
                }

                if (string.Equals(cellType, "inlineStr", StringComparison.OrdinalIgnoreCase))
                {
                    return string.Concat(cellElement.Descendants(s_SpreadsheetNamespace + "t").Select(t => (string)t));
                }

                if (string.Equals(cellType, "b", StringComparison.OrdinalIgnoreCase))
                {
                    return rawValue == "1" ? "true" : "false";
                }

                return rawValue;
            }

            private static XDocument LoadXml(ZipArchive archive, string entryPath)
            {
                ZipArchiveEntry entry = archive.GetEntry(entryPath);
                if (entry == null)
                {
                    throw new InvalidOperationException($"Workbook entry '{entryPath}' was not found.");
                }

                using Stream stream = entry.Open();
                return XDocument.Load(stream);
            }

            private static int ColumnNameToIndex(string columnName)
            {
                int index = 0;
                for (int i = 0; i < columnName.Length; i++)
                {
                    index *= 26;
                    index += columnName[i] - 'A' + 1;
                }

                return index - 1;
            }
        }
    }
}
#endif
