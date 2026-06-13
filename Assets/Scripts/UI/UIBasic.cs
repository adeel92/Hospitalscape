using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBasic : MonoBehaviour
{
    public void Restart()
    {
        Isometric.GameManager.UnPauseGame();
        DG.Tweening.DOTween.KillAll();
        Isometric.CoroutineManager.StopAllCoroutine();
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.scene.name);
    }
}
