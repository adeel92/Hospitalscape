using Isometric.Cam;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] Camera m_TargetCamera;

    [Header("---Gameplay---")]
    [SerializeField] Camera m_GameplayCamera;
    [SerializeField] GameObject m_GameplayUICanvas;
    [SerializeField] UnityEvent m_OnSetupForGameplay;

    [Header("---Menu---")]
    [SerializeField] Camera m_MenuCamera;
    [SerializeField] GameObject m_MenuUICanvas;
    [SerializeField] UnityEvent m_OnSetupForMenu;

    [ContextMenu("Update to Gameplay")]
    public void UpdateToGameplay()
    {
        m_TargetCamera.orthographicSize = m_GameplayCamera.orthographicSize;
        m_TargetCamera.transform.position = m_GameplayCamera.transform.position;
        m_MenuUICanvas.SetActive(false);
        m_GameplayUICanvas.SetActive(true);
        m_OnSetupForGameplay.Invoke();
    }

    [ContextMenu("Update to Menu")]
    public void UpdateToMenu()
    {
        m_TargetCamera.orthographicSize = m_MenuCamera.orthographicSize;
        m_TargetCamera.transform.position = m_MenuCamera.transform.position;
        m_GameplayUICanvas.SetActive(false);
        m_MenuUICanvas.SetActive(true);
        m_OnSetupForMenu.Invoke();
    }
}
#endif