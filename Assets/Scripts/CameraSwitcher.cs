using Isometric.Cam;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
[RequireComponent(typeof(Camera))]
public class CameraSwitcher : MonoBehaviour
{
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
        Camera camera = GetComponent<Camera>();
        camera.orthographicSize = m_GameplayCamera.orthographicSize;
        camera.transform.position = m_GameplayCamera.transform.position;
        m_MenuUICanvas.SetActive(false);
        m_GameplayUICanvas.SetActive(true);
        m_OnSetupForGameplay.Invoke();
    }

    [ContextMenu("Update to Menu")]
    public void UpdateToMenu()
    {
        Camera camera = GetComponent<Camera>();
        camera.orthographicSize = m_MenuCamera.orthographicSize;
        camera.transform.position = m_MenuCamera.transform.position;
        m_GameplayUICanvas.SetActive(false);
        m_MenuUICanvas.SetActive(true);
        m_OnSetupForMenu.Invoke();
    }
}
#endif