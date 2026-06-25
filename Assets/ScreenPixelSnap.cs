using UnityEngine;

[DefaultExecutionOrder(10000)]
public class ScreenPixelSnap : MonoBehaviour
{
    [SerializeField] private Camera gameCamera;

    private void LateUpdate()
    {
        Vector3 screenPoint = gameCamera.WorldToScreenPoint(transform.position);

        screenPoint.x = Mathf.Round(screenPoint.x);
        screenPoint.y = Mathf.Round(screenPoint.y);

        transform.position = gameCamera.ScreenToWorldPoint(screenPoint);
    }
}