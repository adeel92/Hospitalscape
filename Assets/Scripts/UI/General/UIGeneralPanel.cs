using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using Arc.Attribute;

namespace Isometric.UI
{
    /// <summary>
    /// This class is for holding multiple panel chids like button, image etc
    /// If you want to introduce new panel create new enum and its name in the <see cref="GeneralPanelType"/> 
    /// and in the enum name the each holding name
    /// so you can easily access using the Get method by enum 
    /// also send in the EnumStringSelector <see cref="GeneralPanelInfo.HoldingType"/> Parameter
    /// </summary>
    public class UIGeneralPanel : MonoBehaviour
    {
        private enum GeneralPanelComponentType
        {
            Gameobject, Transform, RectTransform, Image, TextMeshProUGUI, Button, NotificationMainUI
        }

        [Serializable]
        private class GeneralPanelInfo
        {
            [EnumStringSelector(nameof(m_GeneralPanelType), 
                typeof(CustomerAchievementPanelType), 
                typeof(CustomerAchievementRightPagePanelType), 
                typeof(AchievementPanelType),
                typeof(AchievementPageType),
                typeof(ShopSpecialOfferPanelType))]
            public string HoldingType;

            [Space]
            public GeneralPanelComponentType ComponentType;

            [AllowNesting, ShowIf(nameof(ComponentType), GeneralPanelComponentType.Gameobject)]
            public GameObject GameobjectHolding;
            [AllowNesting, ShowIf(nameof(ComponentType), GeneralPanelComponentType.Transform)]
            public Transform TransformHolding;
            [AllowNesting, ShowIf(nameof(ComponentType), GeneralPanelComponentType.RectTransform)]
            public RectTransform RectTransformHolding;
            [AllowNesting, ShowIf(nameof(ComponentType), GeneralPanelComponentType.Image)]
            public Image ImageHolding;
            [AllowNesting, ShowIf(nameof(ComponentType), GeneralPanelComponentType.TextMeshProUGUI)]
            public TextMeshProUGUI TextMeshProUGUIHolding;
            [AllowNesting, ShowIf(nameof(ComponentType), GeneralPanelComponentType.Button)]
            public Button ButtonHolding;
            [AllowNesting, ShowIf(nameof(ComponentType), GeneralPanelComponentType.NotificationMainUI)]
            public NotificationMainUI NotificationMain;
        }

        [SerializeField] GeneralPanelType m_GeneralPanelType;
        [SerializeField] List<GeneralPanelInfo> m_GeneralPanelInfo;



        public T GetPanelHolding<T, TEnum>(TEnum holdingName)
            where T : UnityEngine.Object
            where TEnum : Enum
        {
            GeneralPanelInfo generalPanelInfo  = m_GeneralPanelInfo.Find((x) => x.HoldingType == holdingName.ToString());
            if (generalPanelInfo != null)
            {
                if (typeof(T) == typeof(GameObject) && generalPanelInfo.ComponentType== GeneralPanelComponentType.Gameobject)
                {
                    return (T)(object)generalPanelInfo.GameobjectHolding;
                }
                else if (typeof(T) == typeof(Transform) && generalPanelInfo.ComponentType == GeneralPanelComponentType.Transform)
                {
                    return (T)(object)generalPanelInfo.TransformHolding;
                }
                else if (typeof(T) == typeof(Image) && generalPanelInfo.ComponentType == GeneralPanelComponentType.Image)
                {
                    return (T)(object)generalPanelInfo.ImageHolding;
                }
                else if (typeof(T) == typeof(TextMeshProUGUI) && generalPanelInfo.ComponentType == GeneralPanelComponentType.TextMeshProUGUI)
                {
                    return (T)(object)generalPanelInfo.TextMeshProUGUIHolding;
                }
                else if (typeof(T) == typeof(Button) && generalPanelInfo.ComponentType == GeneralPanelComponentType.Button)
                {
                    return (T)(object)generalPanelInfo.ButtonHolding;
                }
                else if (typeof(T) == typeof(NotificationMainUI) && generalPanelInfo.ComponentType == GeneralPanelComponentType.NotificationMainUI)
                {
                    return (T)(object)generalPanelInfo.NotificationMain;
                }
                else
                {
                    Debug.LogWarning("Type of " + typeof(T) + " not found");
                    return default;
                }
            }
            else
            {
                Debug.LogWarning("Holding Name of " + holdingName + " not found");
                return default;
            }
        }

    }

    public enum GeneralPanelType
    {
        CustomerAchievementPanelType, 
        CustomerAchievementRightPagePanelType, 
        AchievementPanelType, 
        AchievementPageType, 
        ShopSpecialOfferPanelType
    }

    public enum CustomerAchievementPanelType
    {
        HeaderNameText, 
        AtLevelUnlockText, 
        CoinRewardHolder, 
        CoinRewardText, 
        GemRewardHolder, 
        GemRewardText, 
        LockedButton, 
        ClaimButton, 
        ClaimedButton, 
        CoinTransform,
        GemTransfrom,
        NotificationMain,
    }

    public enum CustomerAchievementRightPagePanelType
    {
        CustomerImage,
    }

    public enum AchievementPanelType
    {
        PreviewImage, 
        DiscriptionText, 
        BarHolder, 
        BarFillImage, 
        BarText, 
        ClaimButton, 
        LockedGameObject, 
        CoinRewardGameObject, 
        CoinRewardText, 
        GemRewardGameObject,
        GemRewardText,
        ClaimedGameObject,
        CoinTransfrom,
        GemTransfrom,
        NotificationMain
    }

    public enum AchievementPageType
    {
        HolderTransform,
    }

    public enum ShopSpecialOfferPanelType
    {
        PreviewImage, 
        OffText, 
        MainHeadingText, 
        CoinHolder, 
        CoinText, 
        GemHolder, 
        GemText, 
        HeartHolder, 
        HeartText, 
        SpeedBoosterHolder,
        SpeedBoosterText, 
        FrozenBoosterHolder, 
        FrozenBoosterText, 
        OrderFillBoosterHolder, 
        OrderFillBoosterText 
    }
}
