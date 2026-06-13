using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using UnityEngine;
using NaughtyAttributes;
using Arc.Attribute;
using Isometric.UI;


namespace Isometric.Data
{
    [CreateAssetMenu(fileName = "DataProfile", menuName = "GameData/DataProfile")]
    public class DataProfile : DataSaver
    {
        public ProfileData ProfileData => m_DataProfile;
        [SerializeField] ProfileData m_DataProfile;
        [SerializeField] ProfileAvatarHolderUI m_ProfileAvatarPrefab;
        [SerializeField] Color m_ProfileAvatarUnSelectedColor;
        [SerializeField] ProfileFrameHolderUI m_ProfileFramePrefab;
        [SerializeField] Color m_ProfileFrameUnSelectedColor;

        public override void Setup()
        {
            if (FileExists())
            {
                Load();
                Save();
            }
            else
            {
                SetDataToDefault();
                Save();
            }
        }

        public override void SetDataToDefault()
        {
            m_DataProfile.PlayerName = m_DataProfile.PlayerNameDefaultValue;

            m_DataProfile.ProfileAvatarSelectedIndex = m_DataProfile.ProfileAvatarSelectedIndexDefaultValue;
            m_DataProfile.ProfileFramesSelectedIndex = m_DataProfile.ProfileFramesSelectedIndexDefaultValue;

            foreach (var profileAvatarsInfo in m_DataProfile.ProfileAvatarsInfo)
            {
                profileAvatarsInfo.IsUnlocked = profileAvatarsInfo.IsUnlockedDefaultVaue;
            }

            foreach (var profileFramesInfo in m_DataProfile.ProfileFramesInfo)
            {
                profileFramesInfo.IsUnlocked = profileFramesInfo.IsUnlockedDefaultVaue;
            }
        }

        [ContextMenu("Save Data")]
        public void Save()
        {
            SaveData(m_DataProfile);
        }

        [ContextMenu("Load Data")]
        public void Load()
        {
            ProfileData profileData = LoadData<ProfileData>();
            if (profileData != null)
            {
                m_DataProfile.PlayerName = profileData.PlayerName;

                m_DataProfile.ProfileAvatarSelectedIndex = profileData.ProfileAvatarSelectedIndex;
                m_DataProfile.ProfileFramesSelectedIndex = profileData.ProfileFramesSelectedIndex;

                for (int i = 0; i < profileData.ProfileAvatarsInfo.Count && i < m_DataProfile.ProfileAvatarsInfo.Count; i++)
                {
                    m_DataProfile.ProfileAvatarsInfo[i].IsUnlocked = profileData.ProfileAvatarsInfo[i].IsUnlocked;
                }

                for (int i = 0; i < profileData.ProfileFramesInfo.Count && i < m_DataProfile.ProfileFramesInfo.Count; i++)
                {
                    m_DataProfile.ProfileFramesInfo[i].IsUnlocked = profileData.ProfileFramesInfo[i].IsUnlocked;
                }
            }
        }


        public List<ProfileAvatarHolderUI> GetPrfileAvatars(Transform holder, ProfileUIManager profileUIManager)
        {
            List<ProfileAvatarHolderUI> profileAvatars = new List<ProfileAvatarHolderUI>();

            int avtarSelectedIndex = m_DataProfile.ProfileAvatarSelectedIndex;

            int count = 0;
            foreach (var profileAvatarInfo in m_DataProfile.ProfileAvatarsInfo)
            {
                ProfileAvatarHolderUI profileAvatar = Instantiate(m_ProfileAvatarPrefab, holder);
                profileAvatars.Add(profileAvatar);

                profileAvatar.Setup(profileUIManager, this, profileAvatarInfo);

                profileAvatar.BackgroundImage.sprite = profileAvatarInfo.BackgroundSprite;
                profileAvatar.PreviewImage.sprite = profileAvatarInfo.PreviewSprite;

                profileAvatar.PreviewImage.rectTransform.anchoredPosition = profileAvatarInfo.PreviewImageAnchorPosition;
                profileAvatar.PreviewImage.rectTransform.sizeDelta = profileAvatarInfo.PreviewImageDeltaSize;

                profileAvatar.Selected.SetActive(false);

                if (profileAvatarInfo.IsUnlocked == false)
                {
                    profileAvatar.BackgroundImage.color = m_ProfileAvatarUnSelectedColor;
                    profileAvatar.PreviewImage.color = m_ProfileAvatarUnSelectedColor;
                }

                if (avtarSelectedIndex == count)
                {
                    profileAvatar.Tick.SetActive(true);
                    profileAvatar.SelectProfileAvtar();
                    profileUIManager.SelectedProfilAvtarHolder = profileAvatar;
                }
                else
                {
                    profileAvatar.Tick.SetActive(false);
                }

                count++;
            }

            return profileAvatars;
        }

        public List<ProfileFrameHolderUI> GetPrfileFrames(Transform holder, ProfileUIManager profileUIManager)
        {
            List<ProfileFrameHolderUI> profileFrames = new List<ProfileFrameHolderUI>();

            int framesSelectedIndex = m_DataProfile.ProfileFramesSelectedIndex;

            int count = 0;
            foreach (var profileFrameInfo in m_DataProfile.ProfileFramesInfo)
            {
                ProfileFrameHolderUI profileFrame = Instantiate(m_ProfileFramePrefab, holder);
                profileFrames.Add(profileFrame);

                profileFrame.Setup(profileUIManager, this, profileFrameInfo);

                profileFrame.PreviewImage.sprite = profileFrameInfo.PreviewSprite;
                profileFrame.PreviewImage.rectTransform.anchoredPosition = profileFrameInfo.PreviewImageAnchorPosition;
                profileFrame.PreviewImage.rectTransform.sizeDelta = profileFrameInfo.PreviewImageDeltaSize;

                profileFrame.Selected.SetActive(false);

                if (profileFrameInfo.IsUnlocked == false)
                {
                    profileFrame.PreviewImage.color = m_ProfileAvatarUnSelectedColor;
                }

                if (framesSelectedIndex == count)
                {
                    profileFrame.Tick.SetActive(true);
                    profileFrame.SelectProfileFrame();
                    profileUIManager.SelectedProfilFrameHolder = profileFrame;
                }
                else
                {
                    profileFrame.Tick.SetActive(false);
                }

                count++;
            }

            return profileFrames;
        }
    }

    [Serializable]
    public class ProfileData
    {
        public string PlayerName;

        public int ProfileAvatarSelectedIndex;
        public List<ProfileAvatarInfo> ProfileAvatarsInfo;
        public int ProfileFramesSelectedIndex;
        public List<ProfileFrameInfo> ProfileFramesInfo;

        [XmlIgnore]
        public bool EditDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValue))]
        public string PlayerNameDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValue))]
        public int ProfileAvatarSelectedIndexDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValue))]
        public int ProfileFramesSelectedIndexDefaultValue;

    }

    [Serializable]
    public class ProfileAvatarInfo
    {
        [XmlIgnore, EnumFilter((int)CurrencyType.Coin, (int)CurrencyType.Gem)]
        public CurrencyType UpdateType = CurrencyType.Coin;
        [XmlIgnore, AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Coin)]
        public int RequiredCoins;
        [XmlIgnore, AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Gem)]
        public int RequiredGems;

        [XmlIgnore]
        public Sprite BackgroundSprite;

        [XmlIgnore]
        public Sprite PreviewSprite;
        [XmlIgnore]
        public Vector3 PreviewImageAnchorPosition;
        [XmlIgnore]
        public Vector2 PreviewImageDeltaSize;
        [XmlIgnore]
        public Vector3 SelectedPreviewImageAnchorPosition;
        [XmlIgnore]
        public Vector2 SelectedPreviewImageDeltaSize;

        public bool IsUnlocked;

        [XmlIgnore]
        public bool EditDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValue))]
        public bool IsUnlockedDefaultVaue;
    }

    [Serializable]
    public class ProfileFrameInfo
    {
        [XmlIgnore, EnumFilter((int)CurrencyType.Coin, (int)CurrencyType.Gem)]
        public CurrencyType UpdateType = CurrencyType.Coin;
        [XmlIgnore, AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Coin)]
        public int RequiredCoins;
        [XmlIgnore, AllowNesting, ShowIf(nameof(UpdateType), CurrencyType.Gem)]
        public int RequiredGems;

        [XmlIgnore]
        public Sprite PreviewSprite;
        [XmlIgnore]
        public Vector3 PreviewImageAnchorPosition;
        [XmlIgnore]
        public Vector2 PreviewImageDeltaSize;
        [XmlIgnore]
        public Vector3 SelectedPreviewImageAnchorPosition;
        [XmlIgnore]
        public Vector2 SelectedPreviewImageDeltaSize;

        public bool IsUnlocked;

        [XmlIgnore]
        public bool EditDefaultValue;
        [XmlIgnore, AllowNesting, EnableIf(nameof(EditDefaultValue))]
        public bool IsUnlockedDefaultVaue;
    }
}
