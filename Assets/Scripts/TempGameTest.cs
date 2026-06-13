using System.Collections.Generic;
using Isometric;
using Isometric.Cam;
using Isometric.Environment;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TempGameTest : MonoBehaviour
{
    [SerializeField] List<EnvironmentSetupCaller> m_EnvironmentSetupCallers = new();

    private void Start()
    {
        Invoke(nameof(Setup), 0.1f);
        // Setup();
    }

    private void Setup()
    {
        CameraController.SetupForMenu(() =>
        {
            CameraController.Interactability(true);
            CameraController.SetEnvironemntInteractiblity(false);
        });
        // foreach(EnvironmentSetupCaller environmentSetupCaller in m_EnvironmentSetupCallers)
        // {
        //     environmentSetupCaller.CallMenu();
        // }
    }

    public void Play()
    {
        GameManager.SetupForGameplay();

        CameraController.Interactability(false);
        CameraController.SetupForGameplay(null);
        // CameraController.SetEnvironemntInteractiblity(true);
        // foreach(EnvironmentSetupCaller environmentSetupCaller in m_EnvironmentSetupCallers)
        // {
        //     environmentSetupCaller.CallGameplay();
        // }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
