using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using Isometric.Data;
using Isometric.Cam;
using Isometric.UI;

namespace Isometric.Environment
{
    public class EnvironmentDecorationController : MonoBehaviour
    {
        [SerializeField, Expandable] 
        DataEnvironmentDecoration m_DataEnvironmentDecoration;

        [SerializeField] Vector2 m_CameraFocusPosition;
        [SerializeField] float m_CameraZoom;
        [SerializeField] float m_CameraFocusDuration;

        public UnityEvent OnIsLockedMenu;
        [Header("-Not Called First Time Unlocking")]
        public UnityEvent OnIsUnlockdMenu;
        public UnityEvent OnHasJustUnlockedMenu;


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
                        if (CameraController.NextFocusCamera() == false)
                        {
                            CameraController.SetupForMenu(() =>
                            {
                                UIManager.CheckNextUpdatable();
                            });
                        }

                    }, m_CameraFocusDuration);
                });

                data.HasJustUnlocked = false;
                m_DataEnvironmentDecoration.Save();
            }
        }
    }
}