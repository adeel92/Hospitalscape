using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSwitcher : MonoBehaviour
{
    [Header("---Gameplay---")]
    [SerializeField] Camera m_GameplayCamera;

    [Header("---Menu---")]
    [SerializeField] Camera m_MenuCamera;


    [ContextMenu("Update to Gameplay")]
    public void UpdateToGameplay()
    {
        Camera camera = GetComponent<Camera>();
        camera.orthographicSize = m_GameplayCamera.orthographicSize;
        camera.transform.position = m_GameplayCamera.transform.position;
    }

    [ContextMenu("Update to Menu")]
    public void UpdateToMenu()
    {
        Camera camera = GetComponent<Camera>();
        camera.orthographicSize = m_MenuCamera.orthographicSize;
        camera.transform.position = m_MenuCamera.transform.position;
    }
}
