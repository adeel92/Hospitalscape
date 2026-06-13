using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Arc
{
    public class InAppManager : MonoBehaviour, IDetailedStoreListener
    {
        public static InAppManager s_Instance;

        private IStoreController m_StoreController = null;
        private IExtensionProvider m_StoreExtensionProvider = null;

        private bool m_IsPurchasing = false;
        private Action m_PurchaseCallbackSuccessHolder = null;
        private Action m_PurchaseCallbackFailerHolder = null;

        [Header("---Purchasing Info---")]
        [SerializeField] List<PurchasingPackage> m_PurchasingPackages;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                UnityGameServices.OnInitializationSuccess += InitializePurchasing;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDisable()
        {
            UnityGameServices.OnInitializationSuccess -= InitializePurchasing;
        }

        #region Initialize

        public static void InitializePurchasing()
        {
            if (s_Instance != null)
            {
                if (IsInitialized())
                {
                    return;
                }

                var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

                foreach (var package in s_Instance.m_PurchasingPackages)
                {
                    builder.AddProduct(package.PackageID, package.ProductType);
                }

                UnityPurchasing.Initialize(s_Instance , builder);
            }
            else
            {
                PrintNullError();
            }
        }

        private static bool IsInitialized()
        {
            if (s_Instance != null)
            {
                return s_Instance.m_StoreController != null && s_Instance.m_StoreExtensionProvider != null;
            }
            else
            {
                PrintNullError();
                return false;
            }
        }

        //Interface implemented function
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("OnInitialized: Completed!");

            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
        }

        //Interface implemented function
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }
        #endregion

        #region Already Puchased
        public static bool HasPurchasedNonConsumable(PurchasingPackageType packageType)
        {
            if (s_Instance != null && IsInitialized())
            {
                PurchasingPackage temPurchasingPackage = s_Instance.m_PurchasingPackages.Find((x) => x.PurchasingPackageType == packageType);
                if (temPurchasingPackage != null)
                {
                    var product = s_Instance.m_StoreController.products.WithID(temPurchasingPackage.PackageID);
                    if (product != null)
                    {
                        if (product != null && product.hasReceipt)
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                PrintNullError();
            }

            return false;
        }
        #endregion

        #region Purchase
        public static void Purchase(PurchasingPackageType packageType, Action callbackSuccess, Action callbackFailure)
        {
            if (s_Instance != null)
            {
                try
                {
                    if (IsInitialized())
                    {
                        PurchasingPackage temPurchasingPackage = s_Instance.m_PurchasingPackages.Find((x) => x.PurchasingPackageType == packageType);

                        Product product = s_Instance.m_StoreController.products.WithID(temPurchasingPackage.PackageID);

                        if (product != null && product.availableToPurchase)
                        {
                            ResetPurchsingConditions();
                            s_Instance.m_IsPurchasing = true;
                            //s_Instance.m_UseUI = useUI;
                            s_Instance.m_PurchaseCallbackSuccessHolder = callbackSuccess;
                            s_Instance.m_PurchaseCallbackFailerHolder = callbackFailure;
                            Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
                            s_Instance.m_StoreController.InitiatePurchase(product);
                        }
                        else
                        {
                            Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                        }
                    }
                    else
                    {
                        Debug.Log("BuyProductID FAIL. Not initialized.");
                    }
                }
                catch (Exception e)
                {
                    ResetPurchsingConditions();
                    Debug.Log("BuyProductID: FAIL. Exception during purchase. " + e);
                }
            }
            else
            {
#if UNITY_EDITOR
                callbackSuccess?.Invoke();
#endif
                PrintNullError();
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            if (s_Instance != null && s_Instance.m_IsPurchasing)
            {
                s_Instance.m_PurchaseCallbackSuccessHolder?.Invoke();
            }

            ResetPurchsingConditions();

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
            if (s_Instance != null && s_Instance.m_IsPurchasing)
            {
                s_Instance.m_PurchaseCallbackFailerHolder?.Invoke();
            }
            ResetPurchsingConditions();
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureDescription));
            if (s_Instance != null && s_Instance.m_IsPurchasing)
            {
                s_Instance.m_PurchaseCallbackFailerHolder?.Invoke();
            }
            ResetPurchsingConditions();
        }
        #endregion

        #region Purchase Product Price
        /// <summary>
        /// Returns Localized Price if not found empty string
        /// </summary>
        public static string GetLocalizedPrice(PurchasingPackageType packageType)
        {
            string price = "Buy";

            if (s_Instance == null)
            {
                PrintNullError();
                return price;
            }

            if (!IsInitialized())
            {
                return price;
            }

            PurchasingPackage temPurchasingPackage = s_Instance.m_PurchasingPackages.Find((x) => x.PurchasingPackageType == packageType);
            if (temPurchasingPackage != null)
            {
                var product = s_Instance.m_StoreController.products.WithID(temPurchasingPackage.PackageID);
                if (product != null && product.metadata != null)
                {
                    price = product.metadata.localizedPriceString;
                }
            }

            return price;
        }
        #endregion

        #region Helper
        private static void ResetPurchsingConditions()
        {
            s_Instance.m_IsPurchasing = true;
            //s_Instance.m_UseUI = false;
            s_Instance.m_PurchaseCallbackSuccessHolder = null;
            s_Instance.m_PurchaseCallbackFailerHolder = null;
        }

        private static void PrintNullError()
        {
            Debug.LogWarning("InAppManager Instance is null");
        }
        #endregion
    }

    [Serializable]
    public class PurchasingPackage
    {
        public PurchasingPackageType PurchasingPackageType;
        public string PackageID;
        public ProductType ProductType;
    }

    public enum PurchasingPackageType
    {
        BaseRestaurantPack,
        PortableRestaurantPack,
        EfficientRestaurantPack,
        VersatileRestaurantPack,
        GemPack1,
        GemPack2,
        GemPack3,
        GemPack4,
        GemPack5,
        GemPack6,
        NoAdsPack,
        RevivePack
    }
}