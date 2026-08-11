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

        #region EDITOR DATA UPDATE AUTOMATION
        #if UNITY_EDITOR
        [Space, Header("---Editor Design Setup---")]
        [InfoBox("Update or Retrieve the design data for this design index", EInfoBoxType.Normal)]
        [Space, SerializeField, MinValue(0)] int m_EditorDesignIndex;
        [SerializeField] Sprite m_EditorUISprite;

        [Button("Update DataEnvironmentDecoration")]
        private void CaptureCurrentSpritesToSODesign()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Update Failed", "DataEnvironmentDecoration can only be updated in Edit Mode.", "OK");
                return;
            }

            if (m_DataEnvironmentDecoration == null)
            {
                EditorUtility.DisplayDialog("Update Failed", "DataEnvironmentDecoration is not assigned.", "OK");
                return;
            }

            if (m_EditorUISprite == null)
            {
                EditorUtility.DisplayDialog("Update Failed", $"UI Sprite is not assigned for design index {m_EditorDesignIndex}.", "OK");
                return;
            }

            for (int i = 0; i < m_DecorationSubItemRendererInfos.Count; i++)
            {
                DecorationSubItemRendererInfo rendererInfo = m_DecorationSubItemRendererInfos[i];

                for (int j = 0; j < rendererInfo.SpriteRenderers.Count; j++)
                {
                    SpriteRenderer spriteRenderer = rendererInfo.SpriteRenderers[j];

                    if (spriteRenderer == null)
                    {
                        EditorUtility.DisplayDialog("Update Failed", $"SpriteRenderer is null for {rendererInfo.SubItemKey}, renderer index {j}.", "OK");
                        return;
                    }

                    if (spriteRenderer.sprite == null)
                    {
                        EditorUtility.DisplayDialog("Update Failed", $"Sprite is null on SpriteRenderer '{spriteRenderer.name}' for {rendererInfo.SubItemKey}.", "OK");
                        return;
                    }
                }
            }

            HashSet<DecorationSubItem> capturedKeys = new HashSet<DecorationSubItem>();
            List<DecorationSubItemInfo> capturedSubItems = new List<DecorationSubItemInfo>(m_DecorationSubItemRendererInfos.Count);

            for (int i = 0; i < m_DecorationSubItemRendererInfos.Count; i++)
            {
                DecorationSubItemRendererInfo rendererInfo = m_DecorationSubItemRendererInfos[i];

                if (!capturedKeys.Add(rendererInfo.SubItemKey))
                {
                    Debug.LogError($"Duplicate SubItemKey '{rendererInfo.SubItemKey}' exists on {name}.", this);
                    return;
                }

                if (!TryGetCurrentRendererSprite(rendererInfo, out Sprite currentSprite))
                    return;

                capturedSubItems.Add(new DecorationSubItemInfo
                {
                    SubItemKey = rendererInfo.SubItemKey,
                    EnvironmentSprite = currentSprite
                });
            }

            Undo.RecordObject(m_DataEnvironmentDecoration, "Update Decoration Design");

            List<DecorationDesignInfo> designInfos = m_DataEnvironmentDecoration.EnvironmentDecorationData.DecorationDesignInfos;

            while (designInfos.Count <= m_EditorDesignIndex)
                designInfos.Add(new DecorationDesignInfo());

            designInfos[m_EditorDesignIndex] = new DecorationDesignInfo
            {
                UISprite = m_EditorUISprite,
                DecorationSubItemInfos = capturedSubItems
            };

            EditorUtility.SetDirty(m_DataEnvironmentDecoration);
            AssetDatabase.SaveAssets();

            Debug.Log($"Updated design index {m_EditorDesignIndex} with {capturedSubItems.Count} sub-items in {m_DataEnvironmentDecoration.name}.", m_DataEnvironmentDecoration);
        }

        private bool TryGetEditorDesign(out DecorationDesignInfo designInfo)
        {
            designInfo = null;

            if (m_DataEnvironmentDecoration == null)
            {
                Debug.LogWarning($"DataEnvironmentDecoration is missing on {name}.", this);
                return false;
            }

            List<DecorationDesignInfo> designs = m_DataEnvironmentDecoration.EnvironmentDecorationData.DecorationDesignInfos;

            if (m_EditorDesignIndex < 0 || m_EditorDesignIndex >= designs.Count)
            {
                Debug.LogWarning($"Invalid editor design index " + $"{m_EditorDesignIndex} on {name}.", this);
                return false;
            }

            designInfo = designs[m_EditorDesignIndex];
            return true;
        }

        private bool TryGetCurrentRendererSprite(DecorationSubItemRendererInfo rendererInfo, out Sprite currentSprite)
        {
            currentSprite = null;
            bool foundRenderer = false;

            for (int i = 0; i < rendererInfo.SpriteRenderers.Count; i++)
            {
                SpriteRenderer spriteRenderer = rendererInfo.SpriteRenderers[i];

                if (spriteRenderer == null)
                    continue;

                if (!foundRenderer)
                {
                    currentSprite = spriteRenderer.sprite;
                    foundRenderer = true;
                    continue;
                }

                // All renderers under one key must use the same sprite.
                if (spriteRenderer.sprite != currentSprite)
                {
                    Debug.LogError($"Renderers assigned to " + $"'{rendererInfo.SubItemKey}' on {name} " + $"do not have the same sprite.", this);
                    currentSprite = null;
                    return false;
                }
            }

            if (!foundRenderer)
            {
                Debug.LogError($"'{rendererInfo.SubItemKey}' on {name} " + $"does not contain any valid SpriteRenderer.", this);
                return false;
            }

            if (currentSprite == null)
            {
                Debug.LogError($"'{rendererInfo.SubItemKey}' on {name} " + $"has no sprite assigned.", this);
                return false;
            }

            return true;
        }

        [Button("Retrieve Decoration Design")]
        private void ApplySODesignInEditor()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Use this button in Edit Mode.", this);
                return;
            }

            if (!TryGetEditorDesign(out DecorationDesignInfo designInfo))
                return;

            for (int i = 0; i < m_DecorationSubItemRendererInfos.Count; i++)
            {
                DecorationSubItemRendererInfo rendererInfo = m_DecorationSubItemRendererInfos[i];

                if (!TryGetSubItemSprite(designInfo, rendererInfo.SubItemKey, out Sprite environmentSprite))
                {
                    Debug.LogWarning($"Design {m_EditorDesignIndex} does not contain " + $"{rendererInfo.SubItemKey} for {name}.", this);
                    continue;
                }

                for (int rendererIndex = 0; rendererIndex < rendererInfo.SpriteRenderers.Count; rendererIndex++)
                {
                    SpriteRenderer targetRenderer = rendererInfo.SpriteRenderers[rendererIndex];

                    if (targetRenderer == null || targetRenderer.sprite == environmentSprite)
                        continue;

                    Undo.RecordObject(targetRenderer, "Apply Decoration Design");
                    targetRenderer.sprite = environmentSprite;

                    EditorUtility.SetDirty(targetRenderer);
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            Debug.Log($"Applied design index {m_EditorDesignIndex} " + $"to {name}.", this);
        }
        #endif
        #endregion
    }


    [System.Serializable]
    public class DecorationSubItemRendererInfo
    {
        public DecorationSubItem SubItemKey;

        // Supports cases such as two chairs (sub-items) using the same design sprite.
        public List<SpriteRenderer> SpriteRenderers = new();
    }
}