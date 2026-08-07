using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.Cam;
using Isometric.UI;
using UnityEditor;

namespace Isometric.Environment
{
    public class EnvironmentDecorationController : MonoBehaviour
    {
        [SerializeField, Expandable] 
        DataEnvironmentDecoration m_DataEnvironmentDecoration;

        [Space, SerializeField]
        List<DecorationSubItemRendererInfo> m_DecorationSubItemRendererInfos = new();
        [ReadOnly, SerializeField] int m_AppliedDesignIndex = 0;

        [Space]
        [SerializeField] Vector2 m_CameraFocusPosition;
        [SerializeField] float m_CameraZoom;
        [SerializeField] float m_CameraFocusDuration;
        [SerializeField] float m_UnlockingAnimationDuration;

        public UnityEvent OnIsLockedMenu;
        [Header("-Not Called First Time Unlocking")]
        public UnityEvent OnIsUnlockdMenu;
        public UnityEvent OnHasJustUnlockedMenu;


        private void OnEnable()
        {
            GlobalEventHolder.OnStarItemChoiceButtonClick += PreviewDesign;
            GlobalEventHolder.OnStarItemChoiceFinalSelection += SaveSelectedDesign;
        }
        private void OnDisable()
        {
            GlobalEventHolder.OnStarItemChoiceButtonClick -= PreviewDesign;
            GlobalEventHolder.OnStarItemChoiceFinalSelection -= SaveSelectedDesign;
        }

        /* [ContextMenu("temp")]
        public void temp()
        {
            SpriteRenderer spriteRenderer = transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>();
            DecorationSubItemRendererInfo decorationSubItemRendererInfo = new();
            decorationSubItemRendererInfo.SubItemKey = DecorationSubItem.Item1;
            decorationSubItemRendererInfo.SpriteRenderers.Add(spriteRenderer);
            m_DecorationSubItemRendererInfos.Add(decorationSubItemRendererInfo);
            EditorUtility.SetDirty(this);
        } */

        [ContextMenu("SetupForMenu")]
        public void SetupForMenu()
        {
            EnvironmentDecorationData data = m_DataEnvironmentDecoration.EnvironmentDecorationData;

            if (!data.IsUnlocked)
            {
                OnIsLockedMenu?.Invoke();
            }
            else if (data.IsUnlocked && !data.HasJustUnlocked)
            {
                OnIsUnlockdMenu?.Invoke();
                LoadSelectedDesign();
            }

            if (data.HasJustUnlocked)
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
                    OnHasJustUnlockedMenu?.Invoke();
                    CoroutineManager.LateAction(() =>
                    {
                        StarItemUnlockingUIManager starItemUnlockingUIManager = UIManager.GetPopup<StarItemUnlockingUIManager>();
                        starItemUnlockingUIManager.OpenChoicePopup(() =>
                        {
                            UIManager.UIInteractionOn();
                        });
                    }, m_UnlockingAnimationDuration);
                });

                // data.HasJustUnlocked = false;
                // m_DataEnvironmentDecoration.Save();
            }
        }
    
        public void PreviewDesign(int designIndex)
        {
            EnvironmentDecorationData decorationData = m_DataEnvironmentDecoration.EnvironmentDecorationData;

            // No need to preview the design of those decoration items who were not just unlocked
            if(!decorationData.HasJustUnlocked)
                return;

            List<DecorationDesignInfo> designs = decorationData.DecorationDesignInfos;
            if (designIndex < 0 || designIndex >= designs.Count)
            {
                Debug.LogWarning($"Invalid design index {designIndex} on {name}.",this);
                return;
            }

            DecorationDesignInfo selectedDesign = designs[designIndex];
            for (int i = 0; i < m_DecorationSubItemRendererInfos.Count; i++)
            {
                DecorationSubItemRendererInfo rendererInfo = m_DecorationSubItemRendererInfos[i];

                if (!TryGetSubItemSprite(selectedDesign, rendererInfo.SubItemKey, out Sprite environmentSprite))
                {
                    Debug.LogWarning($"Design {designIndex} does not contain " + $"{rendererInfo.SubItemKey} for {name}.", this);
                    continue;
                }

                for (int rendererIndex = 0; rendererIndex < rendererInfo.SpriteRenderers.Count; rendererIndex++)
                {
                    SpriteRenderer targetRenderer = rendererInfo.SpriteRenderers[rendererIndex];
                    targetRenderer.sprite = environmentSprite;
                }
            }
        }

        private bool TryGetSubItemSprite(DecorationDesignInfo designInfo, DecorationSubItem requiredKey, out Sprite environmentSprite)
        {
            List<DecorationSubItemInfo> subItems = designInfo.DecorationSubItemInfos;

            for (int i = 0; i < subItems.Count; i++)
            {
                DecorationSubItemInfo subItemInfo = subItems[i];

                if (subItemInfo.SubItemKey != requiredKey)
                    continue;

                environmentSprite = subItemInfo.EnvironmentSprite;
                return environmentSprite != null;
            }

            environmentSprite = null;
            return false;
        }

        public void SaveSelectedDesign(int designIndex)
        {
            EnvironmentDecorationData decorationData = m_DataEnvironmentDecoration.EnvironmentDecorationData;
            
            // No need to save the design of those decoration items who were not just unlocked
            if(!decorationData.HasJustUnlocked)
                return;

            List<DecorationDesignInfo> designs = decorationData.DecorationDesignInfos;

            if (designIndex < 0 || designIndex >= designs.Count)
            {
                Debug.LogWarning($"Invalid saving design index {designIndex} on {name}.",this);
                return;
            }
            
            m_AppliedDesignIndex = designIndex;
            DataManager.SaveData();
            decorationData.CurrentDesignIndex = designIndex;
            decorationData.HasJustUnlocked = false;
            m_DataEnvironmentDecoration.Save();

            UIManager.UIInteractionOff();
            StarItemUnlockingUIManager starItemUnlockingUIManager = UIManager.GetPopup<StarItemUnlockingUIManager>();
            starItemUnlockingUIManager.CloseChoicePopup(() =>
            {
                if (CameraController.NextFocusCamera() == false)
                {
                    CameraController.SetupForMenu(() =>
                    {
                        UIManager.CheckNextUpdatable();
                    });
                }
            });
        }
        public void LoadSelectedDesign()
        {
            EnvironmentDecorationData decorationData = m_DataEnvironmentDecoration.EnvironmentDecorationData;
            int savedDesignIndex = decorationData.CurrentDesignIndex;

            if(savedDesignIndex == m_AppliedDesignIndex)
                return;

            List<DecorationDesignInfo> designs = decorationData.DecorationDesignInfos;
            if (savedDesignIndex < 0 || savedDesignIndex >= designs.Count)
            {
                Debug.LogWarning($"Invalid design index {savedDesignIndex} on {name}.",this);
                return;
            }

            DecorationDesignInfo selectedDesign = designs[savedDesignIndex];
            for (int i = 0; i < m_DecorationSubItemRendererInfos.Count; i++)
            {
                DecorationSubItemRendererInfo rendererInfo = m_DecorationSubItemRendererInfos[i];

                if (!TryGetSubItemSprite(selectedDesign, rendererInfo.SubItemKey, out Sprite environmentSprite))
                {
                    Debug.LogWarning($"Design {savedDesignIndex} does not contain " + $"{rendererInfo.SubItemKey} for {name}.", this);
                    continue;
                }

                for (int rendererIndex = 0; rendererIndex < rendererInfo.SpriteRenderers.Count; rendererIndex++)
                {
                    SpriteRenderer targetRenderer = rendererInfo.SpriteRenderers[rendererIndex];
                    targetRenderer.sprite = environmentSprite;
                }
            }

            m_AppliedDesignIndex = decorationData.CurrentDesignIndex;
        }
    }


    [System.Serializable]
    public class DecorationSubItemRendererInfo
    {
        public DecorationSubItem SubItemKey;

        // Supports cases such as two chairs (sub-items) using the same design sprite.
        public List<SpriteRenderer> SpriteRenderers = new();
    }
}