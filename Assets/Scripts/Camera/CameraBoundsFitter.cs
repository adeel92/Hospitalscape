using UnityEngine;

namespace Isometric.Cam
{
    public class CameraBoundsFitter : MonoBehaviour
    {
        [Header("---Camera---")]
        [SerializeField] private Camera targetCamera;

        [Header("---Reference Bounds---")]
        [SerializeField] private Transform leftCenter;
        [SerializeField] private Transform rightCenter;
        [SerializeField] private Transform topCenter;
        [SerializeField] private Transform bottomCenter;

        [Header("---Sizing---")]
        [SerializeField] private float minOrthoSize = 1f;
        [SerializeField] private float sizeStep = 0.01f;
        [SerializeField] private float maxOrthoSize = 100f;

        [Header("---Gizmos---")]
        [SerializeField] private Color boundaryColor = Color.red;
        [SerializeField] private Color cameraBoundsColor = Color.cyan;

        private void OnEnable()
        {
            if (!targetCamera)
                targetCamera = Camera.main;

            if (!targetCamera || !targetCamera.orthographic)
            {
                Debug.LogError("CameraBoundsFitter: Target camera is missing or not orthographic.");
                return;
            }

            FitCamera();
        }

        [ContextMenu("FitCamera")]
        private void FitCamera()
        {
            float aspect = targetCamera.aspect;
            Vector3 camPos = targetCamera.transform.position;

            float orthoSize = minOrthoSize;

            while (orthoSize < maxOrthoSize)
            {
                float verticalExtent = orthoSize;               //half height
                float horizontalExtent = orthoSize * aspect;    //half width with respect to half height and current aspect

                float camLeft = camPos.x - horizontalExtent;
                float camRight = camPos.x + horizontalExtent;
                float camTop = camPos.y + verticalExtent;
                float camBottom = camPos.y - verticalExtent;

                bool exceeds =
                    camLeft < leftCenter.position.x &&
                    camRight > rightCenter.position.x &&
                    camTop > topCenter.position.y &&
                    camBottom < bottomCenter.position.y;

                if (exceeds)
                {
                    orthoSize -= sizeStep;
                    break;
                }

                orthoSize += sizeStep;
            }

            targetCamera.orthographicSize = Mathf.Max(minOrthoSize, orthoSize);
        }

    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!leftCenter || !rightCenter || !topCenter || !bottomCenter)
                return;

            // ---------- Imaginary Boundary ----------
            Gizmos.color = boundaryColor;

            Vector3 topLeft = new Vector3(leftCenter.position.x, topCenter.position.y, 0f);
            Vector3 topRight = new Vector3(rightCenter.position.x, topCenter.position.y, 0f);
            Vector3 bottomRight = new Vector3(rightCenter.position.x, bottomCenter.position.y, 0f);
            Vector3 bottomLeft = new Vector3(leftCenter.position.x, bottomCenter.position.y, 0f);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

            // ---------- Camera Bounds Preview ----------
            if (targetCamera && targetCamera.orthographic)
            {
                Gizmos.color = cameraBoundsColor;

                float verticalExtent = targetCamera.orthographicSize;
                float horizontalExtent = verticalExtent * targetCamera.aspect;
                Vector3 camPos = targetCamera.transform.position;

                Vector3 camTL = new Vector3(camPos.x - horizontalExtent, camPos.y + verticalExtent, 0f);
                Vector3 camTR = new Vector3(camPos.x + horizontalExtent, camPos.y + verticalExtent, 0f);
                Vector3 camBR = new Vector3(camPos.x + horizontalExtent, camPos.y - verticalExtent, 0f);
                Vector3 camBL = new Vector3(camPos.x - horizontalExtent, camPos.y - verticalExtent, 0f);

                Gizmos.DrawLine(camTL, camTR);
                Gizmos.DrawLine(camTR, camBR);
                Gizmos.DrawLine(camBR, camBL);
                Gizmos.DrawLine(camBL, camTL);
            }
        }
    #endif
    }
}