using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using DG.Tweening;
using NaughtyAttributes;
using Isometric.Sound;

namespace Isometric.Cam
{
    public class CameraController : MonoBehaviour
    {
        private static CameraController s_Instance;

        private class CameraFocusInfo
        {
            public Vector3 Position;
            public float Zoom;
            public float Duration = 1.4f;
            public Action OnStart = null;
            public Action OnComplete = null;
        }

        [Header("---Setup---")]
        [SerializeField] Camera m_Camera;
        [SerializeField] Physics2DRaycaster m_Physics2DRaycaster;
        [Space, SerializeField] bool m_TestBounds = false;
        [ShowIf(nameof(m_TestBounds)), SerializeField] bool m_ForMenu = false;
        [ShowIf(nameof(m_TestBounds)), SerializeField] bool m_ForGameplay = true;

        [Header("---Menu Zoom---")]
        [SerializeField] bool m_UseMenuZoomBoundMethod;
        [SerializeField, ShowIf(nameof(m_UseMenuZoomBoundMethod))] bool m_ShowMenuZoomVisual = true;
        [SerializeField, ShowIf(nameof(m_UseMenuZoomBoundMethod))] private Transform m_MenuZoomLeftCenter;
        [SerializeField, ShowIf(nameof(m_UseMenuZoomBoundMethod))] private Transform m_MenuZoomRightCenter;
        [SerializeField, ShowIf(nameof(m_UseMenuZoomBoundMethod))] private Transform m_MenuZoomTopCenter;
        [SerializeField, ShowIf(nameof(m_UseMenuZoomBoundMethod))] private Transform m_MenuZoomBottomCenter;
        [SerializeField, ShowIf(nameof(m_UseMenuZoomBoundMethod))] private Color m_MenuZoomBoundaryColor = Color.red;
        [SerializeField, ShowIf(nameof(m_UseMenuZoomBoundMethod))] private float m_MenuZoomSizeStep = 0.01f;
        [SerializeField] float m_MenuCameraSetDuration;
        [SerializeField, DisableIf(nameof(m_UseMenuZoomBoundMethod))] Vector3 m_MenuCameraPosition;
        [SerializeField, DisableIf(nameof(m_UseMenuZoomBoundMethod))] float m_MenuZoom;

        [Header("---Gameplay Zoom---")]
        [SerializeField] bool m_UseGameplayZoomBoundMethod;
        [SerializeField, ShowIf(nameof(m_UseGameplayZoomBoundMethod))] bool m_ShowGameplayZoomVisual = true;
        [SerializeField, ShowIf(nameof(m_UseGameplayZoomBoundMethod))] Transform m_GameplayZoomLeftCenter;
        [SerializeField, ShowIf(nameof(m_UseGameplayZoomBoundMethod))] Transform m_GameplayZoomRightCenter;
        [SerializeField, ShowIf(nameof(m_UseGameplayZoomBoundMethod))] Transform m_GameplayZoomTopCenter;
        [SerializeField, ShowIf(nameof(m_UseGameplayZoomBoundMethod))] Transform m_GameplayZoomBottomCenter;
        [SerializeField, ShowIf(nameof(m_UseGameplayZoomBoundMethod))] Color m_GameplayZoomBoundaryColor = Color.green;
        [SerializeField, ShowIf(nameof(m_UseGameplayZoomBoundMethod))] float m_GameplayZoomSizeStep = 0.01f;
        [SerializeField] float m_GameplayCameraSetDuration;
        [SerializeField, DisableIf(nameof(m_UseGameplayZoomBoundMethod))] Vector3 m_GameplayCameraPosition;
        [SerializeField, DisableIf(nameof(m_UseGameplayZoomBoundMethod))] float m_GameplayZoom;


        [Header("---Parameters---")]
        [SerializeField] bool m_CameraIsInteractable;
        [SerializeField] float m_MoveSmoothness = 10f;
        private Vector3 m_CurrentVelocity = Vector3.zero;
        private Vector3 m_DragDelta = Vector3.zero;
        private bool m_IsGliding = false;
        private float m_GlideTime = 0.5f;
        private float m_GlideTimer = 0f;

        private bool m_CameraIsInteractableTutorial = true;

        [SerializeField] float m_ZoomSpeed = 5f;

        [SerializeField] Transform m_MapBottomLeft;
        [SerializeField] Transform m_MapTopRight;
        [SerializeField, ReadOnly] Vector2 m_MinLimits;
        [SerializeField, ReadOnly] Vector2 m_MaxLimits;

        [SerializeField] float m_MinZoom = 5f;
        [SerializeField] float m_MaxZoom = 15f;

        private Vector3 m_TouchStart;
        private Vector3 m_MouseStart;
        private float m_TouchZoomStart;
        private bool m_ShouldDrag = false;

        private List<CameraFocusInfo> m_CameraFocusInfoQueue = null;
        private CameraFocusInfo m_CameraFocusCurrentInfo = null;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                m_MaxZoom = CalculateMaxZoom(); // Recalculate max zoom at startup
                if(m_UseMenuZoomBoundMethod) CalculateMenuZoom();
                if(m_UseGameplayZoomBoundMethod) CalculateGameplayZoom();
                m_GameplayZoom = Mathf.Clamp(m_GameplayZoom, m_MinZoom, m_MaxZoom);
                m_MenuZoom = Mathf.Clamp(m_MenuZoom, m_MinZoom, m_MaxZoom);
                m_Camera.orthographicSize = m_GameplayZoom;
                UpdateCameraLimits();
                m_CameraFocusInfoQueue = new List<CameraFocusInfo>();
            }
        }

#if UNITY_EDITOR
        private void LateUpdate()
        {
            if (m_TestBounds)
            {
                m_MaxZoom = CalculateMaxZoom(); // Recalculate max zoom at startup
                UpdateCameraLimits();
                m_CameraFocusInfoQueue = new List<CameraFocusInfo>();

                if (m_ForMenu)
                {
                    if(m_UseMenuZoomBoundMethod) CalculateMenuZoom();
                    m_MenuZoom = Mathf.Clamp(m_MenuZoom, m_MinZoom, m_MaxZoom);
                    SetupForMenu(null);
                }
                else if (m_ForGameplay)
                {
                    if(m_UseGameplayZoomBoundMethod) CalculateGameplayZoom();
                    m_GameplayZoom = Mathf.Clamp(m_GameplayZoom, m_MinZoom, m_MaxZoom);
                    m_Camera.orthographicSize = m_GameplayZoom;
                    SetupForGameplay(null);
                }
            }
        }
#endif


        public static void SetupForMenu(Action onComplete)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            Sequence cameraMenuSequence = DOTween.Sequence();

            cameraMenuSequence.Insert(0,
                s_Instance.m_Camera.transform.DOMove(s_Instance.m_MenuCameraPosition, s_Instance.m_MenuCameraSetDuration)
            );

            cameraMenuSequence.Insert(0,
                s_Instance.m_Camera.DOOrthoSize(s_Instance.m_MenuZoom, s_Instance.m_MenuCameraSetDuration)
            );

            cameraMenuSequence.OnComplete(() => 
            {
                s_Instance.UpdateCameraLimits();
                onComplete?.Invoke(); 
            });
        }

        public static void SetupForGameplay(Action onComplete)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }


            Sequence cameraGameplaySequence = DOTween.Sequence();

            SoundManager.PlaySound(SoundType.CameraWhoosh);
            cameraGameplaySequence.Insert(0, 
                s_Instance.m_Camera.transform.DOMove(s_Instance.m_GameplayCameraPosition, s_Instance.m_GameplayCameraSetDuration)
            );

            cameraGameplaySequence.Insert(0,
                s_Instance.m_Camera.DOOrthoSize(s_Instance.m_GameplayZoom, s_Instance.m_MenuCameraSetDuration)
            );

            cameraGameplaySequence.OnComplete(() => 
            {
                s_Instance.UpdateCameraLimits();
                onComplete?.Invoke(); 
            });
        }

        private void CalculateMenuZoom()
        {
            Vector3 sum = m_MenuZoomLeftCenter.position + m_MenuZoomRightCenter.position + m_MenuZoomTopCenter.position + m_MenuZoomBottomCenter.position;
            Vector3 cernterPoint = sum / 4;
            float aspect = m_Camera.aspect;

            float orthoSize = m_MinZoom;

            while (orthoSize < m_MaxZoom)
            {
                float verticalExtent = orthoSize;
                float horizontalExtent = orthoSize * aspect;

                float camLeft = cernterPoint.x - horizontalExtent;
                float camRight = cernterPoint.x + horizontalExtent;
                float camTop = cernterPoint.y + verticalExtent;
                float camBottom = cernterPoint.y - verticalExtent;

                bool exceeds =
                    camLeft < m_MenuZoomLeftCenter.position.x &&
                    camRight > m_MenuZoomRightCenter.position.x &&
                    camTop > m_MenuZoomTopCenter.position.y &&
                    camBottom < m_MenuZoomBottomCenter.position.y;

                if (exceeds)
                {
                    orthoSize -= m_MenuZoomSizeStep;
                    break;
                }

                orthoSize += m_MenuZoomSizeStep;
            }

            m_MenuCameraPosition = cernterPoint;
            m_MenuCameraPosition.z = m_Camera.transform.position.z;
            m_MenuZoom = Mathf.Max(m_MinZoom, orthoSize);  
        }

        private void CalculateGameplayZoom()
        {
            Vector3 sum = m_GameplayZoomLeftCenter.position + m_GameplayZoomRightCenter.position + m_GameplayZoomTopCenter.position + m_GameplayZoomBottomCenter.position;
            Vector3 cernterPoint = sum / 4;
            float aspect = m_Camera.aspect;

            float orthoSize = m_MinZoom;

            while (orthoSize < m_MaxZoom)
            {
                float verticalExtent = orthoSize;               //half height
                float horizontalExtent = orthoSize * aspect;    //half width with respect to half height and current aspect

                float camLeft = cernterPoint.x - horizontalExtent;
                float camRight = cernterPoint.x + horizontalExtent;
                float camTop = cernterPoint.y + verticalExtent;
                float camBottom = cernterPoint.y - verticalExtent;

                bool exceeds =
                    camLeft < m_GameplayZoomLeftCenter.position.x &&
                    camRight > m_GameplayZoomRightCenter.position.x &&
                    camTop > m_GameplayZoomTopCenter.position.y &&
                    camBottom < m_GameplayZoomBottomCenter.position.y;

                if (exceeds)
                {
                    orthoSize -= m_GameplayZoomSizeStep;
                    break;
                }

                orthoSize += m_GameplayZoomSizeStep;
            }

            m_GameplayCameraPosition = cernterPoint;
            m_GameplayCameraPosition.z = m_Camera.transform.position.z;
            m_GameplayZoom = Mathf.Max(m_MinZoom, orthoSize);  
        }

        public static void SetEnvironemntInteractiblity(bool value)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_Physics2DRaycaster.enabled = value;
        }

        public static Camera GetCamera()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return null;
            }

            return s_Instance.m_Camera;
        }

        public static void Interactability(bool value)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_CameraIsInteractable = value;
        }

        public static void InteractabilityForTutorial(bool value)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instance.m_CameraIsInteractableTutorial = value;
        }

        public static void RegisterFocusCamera(Vector3 position, float zoom, float duration = 1.4f, Action onStart = null, Action onComplete = null)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            CameraFocusInfo focusInfo = new CameraFocusInfo();
            focusInfo.Position = position;
            focusInfo.Zoom = zoom;
            focusInfo.Duration = duration;
            focusInfo.OnStart = onStart;
            focusInfo.OnComplete = onComplete;

            s_Instance.m_CameraFocusInfoQueue.Add(focusInfo);

            if (s_Instance.m_CameraFocusCurrentInfo == null)
            {
                s_Instance.m_CameraFocusCurrentInfo = s_Instance.m_CameraFocusInfoQueue[s_Instance.m_CameraFocusInfoQueue.Count - 1];
                FocusCamera(s_Instance.m_CameraFocusCurrentInfo.Position,
                    s_Instance.m_CameraFocusCurrentInfo.Zoom,
                    s_Instance.m_CameraFocusCurrentInfo.Duration,
                    s_Instance.m_CameraFocusCurrentInfo.OnStart,
                    s_Instance.m_CameraFocusCurrentInfo.OnComplete);
            }
        }

        //Focuses on next registered focus and returns if it has next registered focus
        public static bool NextFocusCamera()
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return false;
            }

            if (s_Instance.m_CameraFocusCurrentInfo != null)
            {
                s_Instance.m_CameraFocusInfoQueue.Remove(s_Instance.m_CameraFocusCurrentInfo);
                s_Instance.m_CameraFocusCurrentInfo = null;

                if (s_Instance.m_CameraFocusInfoQueue.Count > 0)
                {
                    s_Instance.m_CameraFocusCurrentInfo = s_Instance.m_CameraFocusInfoQueue[s_Instance.m_CameraFocusInfoQueue.Count - 1];
                    FocusCamera(s_Instance.m_CameraFocusCurrentInfo.Position,
                        s_Instance.m_CameraFocusCurrentInfo.Zoom,
                        s_Instance.m_CameraFocusCurrentInfo.Duration,
                        s_Instance.m_CameraFocusCurrentInfo.OnStart,
                        s_Instance.m_CameraFocusCurrentInfo.OnComplete);

                    return true;
                }
            }

            return false;
        }

        private static void FocusCamera(Vector3 position, float zoom, float duration = 1.4f, Action onStart = null, Action onComplete = null)
        {
            if (s_Instance == null)
            {
                PrintNullInstanceError();
                return;
            }

            float vertExtent = zoom;
            float horzExtent = vertExtent * s_Instance.m_Camera.aspect;

            Vector2 minLimits = new Vector2(
                s_Instance.m_MapBottomLeft.position.x + horzExtent,
                s_Instance.m_MapBottomLeft.position.y + vertExtent
            );

            Vector2 maxLimits = new Vector2(
                s_Instance.m_MapTopRight.position.x - horzExtent,
                s_Instance.m_MapTopRight.position.y - vertExtent
            );

            // --- Clamp the target position within new limits ---
            Vector3 clampedPosition = new Vector3(
                Mathf.Clamp(position.x, minLimits.x, maxLimits.x),
                Mathf.Clamp(position.y, minLimits.y, maxLimits.y),
                s_Instance.m_Camera.transform.position.z // preserve Z
            );

            // --- Animate camera movement and zoom ---
            Sequence cameraFocusSequence = DOTween.Sequence();

            cameraFocusSequence.InsertCallback(0,
                () => { onStart?.Invoke(); }
            );

            cameraFocusSequence.Insert(0,
                s_Instance.m_Camera.transform.DOMove(clampedPosition, duration)
            );

            cameraFocusSequence.Insert(0,
                s_Instance.m_Camera.DOOrthoSize(zoom, duration)
            );

            cameraFocusSequence.OnComplete(() =>
            {
                s_Instance.UpdateCameraLimits();
                onComplete?.Invoke();
            });
        }

        void Update()
        {
            if (m_CameraIsInteractable == false)
                return;

            if (m_CameraIsInteractableTutorial == false)
                return;

            if (IsPointerOverUI())
                return;

#if UNITY_EDITOR
            HandleMouseMovement();
            HandleDesktopZoom();
#elif UNITY_IOS || UNITY_ANDROID
            HandlePinchZoom(); // Only pinch zoom with two fingers
            HandleTouchMovement(); // Only drag with one finger
#endif


            if (m_IsGliding && m_GlideTimer > 0f)
            {
                Vector3 targetPosition = transform.position + m_DragDelta;
                targetPosition.x = Mathf.Clamp(targetPosition.x, m_MinLimits.x, m_MaxLimits.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, m_MinLimits.y, m_MaxLimits.y);

                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref m_CurrentVelocity, 1f / m_MoveSmoothness);

                m_GlideTimer -= Time.deltaTime;

                // Gradually reduce drag delta to simulate friction
                m_DragDelta = Vector3.Lerp(m_DragDelta, Vector3.zero, Time.deltaTime * 4f);
            }

        }

        void HandleMouseMovement()
        {
            if (Input.GetMouseButtonDown(0))
            {
                m_ShouldDrag = true;
                m_MouseStart = m_Camera.ScreenToWorldPoint(Input.mousePosition);
                m_IsGliding = false;
            }
            else if (Input.GetMouseButton(0) && m_ShouldDrag)
            {
                Vector3 current = m_Camera.ScreenToWorldPoint(Input.mousePosition);
                Vector3 direction = m_MouseStart - current;
                m_DragDelta = direction;
                Vector3 targetPosition = transform.position + direction;

                targetPosition.x = Mathf.Clamp(targetPosition.x, m_MinLimits.x, m_MaxLimits.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, m_MinLimits.y, m_MaxLimits.y);

                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref m_CurrentVelocity, 1f / m_MoveSmoothness);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_ShouldDrag = false;
                m_IsGliding = true;
                m_GlideTimer = m_GlideTime;
            }

        }

        void HandleDesktopZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (Mathf.Abs(scroll) > 0.0001f) // Only proceed if scroll actually occurred
            {
                m_Camera.orthographicSize -= scroll * m_ZoomSpeed;
                m_MaxZoom = CalculateMaxZoom(); // Recalculate max zoom dynamically
                m_Camera.orthographicSize = Mathf.Clamp(m_Camera.orthographicSize, m_MinZoom, m_MaxZoom);

                UpdateCameraLimits();
                ReadjustCameraPositionWithinLimits();
            }
        }

        void HandleTouchMovement()
        {

            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    m_ShouldDrag = true;
                    m_TouchStart = m_Camera.ScreenToWorldPoint(touch.position);
                    m_IsGliding = false;
                }
                else if (touch.phase == TouchPhase.Moved && m_ShouldDrag)
                {
                    Vector3 current = m_Camera.ScreenToWorldPoint(touch.position);
                    Vector3 direction = m_TouchStart - current;
                    m_DragDelta = direction;
                    Vector3 targetPosition = transform.position + direction;

                    targetPosition.x = Mathf.Clamp(targetPosition.x, m_MinLimits.x, m_MaxLimits.x);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, m_MinLimits.y, m_MaxLimits.y);

                    transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref m_CurrentVelocity, 1f / m_MoveSmoothness);
                }
            }
            else if (Input.touchCount == 0 && m_ShouldDrag)
            {
                m_ShouldDrag = false;
                m_IsGliding = true;
                m_GlideTimer = m_GlideTime;
            }

        }

        void HandlePinchZoom()
        {
            if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                Vector2 touchPos1 = touch1.position;
                Vector2 touchPos2 = touch2.position;

                float currentDistance = Vector2.Distance(touchPos1, touchPos2);

                if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
                {
                    m_TouchZoomStart = currentDistance;
                    m_ShouldDrag = false;
                    m_IsGliding = false;
                    m_GlideTimer = 0;
                    m_DragDelta = Vector2.zero;
                }
                else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
                {
                    Vector2 midPoint = (touchPos1 + touchPos2) * 0.5f;
                    Vector3 worldMidBeforeZoom = m_Camera.ScreenToWorldPoint(new Vector3(midPoint.x, midPoint.y, m_Camera.nearClipPlane));

                    float zoomFactor = (m_TouchZoomStart - currentDistance) * m_ZoomSpeed * 0.01f;
                    m_Camera.orthographicSize += zoomFactor;
                    m_Camera.orthographicSize = Mathf.Clamp(m_Camera.orthographicSize, m_MinZoom, m_MaxZoom);
                    m_TouchZoomStart = currentDistance;

                    Vector3 worldMidAfterZoom = m_Camera.ScreenToWorldPoint(new Vector3(midPoint.x, midPoint.y, m_Camera.nearClipPlane));
                    Vector3 worldDelta = worldMidBeforeZoom - worldMidAfterZoom;
                    m_Camera.transform.position += worldDelta;

                    UpdateCameraLimits();
                    ReadjustCameraPositionWithinLimits();
                }
            }
        }


        bool IsPointerOverUI()
        {
            if (EventSystem.current != null)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                return EventSystem.current.IsPointerOverGameObject();
#elif UNITY_IOS || UNITY_ANDROID
                if (Input.touchCount > 0)
                {
                    return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                }
                return false;
#endif
            }
            else
            {
                return false;
            }
        }

        private void UpdateCameraLimits()
        {
            float vertExtent = m_Camera.orthographicSize;
            float horzExtent = vertExtent * m_Camera.aspect;

            m_MinLimits = new Vector2(
                m_MapBottomLeft.position.x + horzExtent,
                m_MapBottomLeft.position.y + vertExtent
            );

            m_MaxLimits = new Vector2(
                m_MapTopRight.position.x - horzExtent,
                m_MapTopRight.position.y - vertExtent
            );

            Vector2 mapBoundaryCenter = (m_MapBottomLeft.position + m_MapTopRight.position) / 2;
            
            if (m_MinLimits.x >= m_MaxLimits.x)
            {
                // m_MinLimits.x = 0f;
                // m_MaxLimits.x = 0f;
                m_MinLimits.x = mapBoundaryCenter.x;
                m_MaxLimits.x = mapBoundaryCenter.x;
            }

            if (m_MinLimits.y >= m_MaxLimits.y)
            {
                // m_MinLimits.y = 0f;
                // m_MaxLimits.y = 0f;
                m_MinLimits.y = mapBoundaryCenter.y;
                m_MaxLimits.y = mapBoundaryCenter.y;
            }
        }

        // private void ReadjustCameraPositionWithinLimits()
        // {
        //     Vector3 targetPosition = transform.position + m_DragDelta;
        //     targetPosition.x = Mathf.Clamp(targetPosition.x, m_MinLimits.x, m_MaxLimits.x);
        //     targetPosition.y = Mathf.Clamp(targetPosition.y, m_MinLimits.y, m_MaxLimits.y);

        //     transform.position = targetPosition;
        // }
        private void ReadjustCameraPositionWithinLimits()
        {
            Vector3 targetPosition = transform.position;
            targetPosition.x = Mathf.Clamp(targetPosition.x, m_MinLimits.x, m_MaxLimits.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, m_MinLimits.y, m_MaxLimits.y);

            transform.position = targetPosition;
        }

        // private float CalculateMaxZoom()
        // {
        //     float mapWidth = m_MapTopRight.position.x - m_MapBottomLeft.position.x;
        //     float mapHeight = m_MapTopRight.position.y - m_MapBottomLeft.position.y;

        //     float maxZoomWidth = mapWidth / (2f * m_Camera.aspect);
        //     float maxZoomHeight = mapHeight / 2f;
        //     float maxZoom = Mathf.Min(maxZoomWidth, maxZoomHeight);
        //     return Mathf.Max(m_GameplayZoom, maxZoom);
        // }
        private float CalculateMaxZoom()
        {
            float mapWidth = m_MapTopRight.position.x - m_MapBottomLeft.position.x;
            float mapHeight = m_MapTopRight.position.y - m_MapBottomLeft.position.y;

            float maxZoomWidth = mapWidth / (2f * m_Camera.aspect);
            float maxZoomHeight = mapHeight / 2f;

            // The camera can only zoom out until BOTH viewport width and height fit inside the map.
            // Do not force this to m_GameplayZoom, because that can make the viewport wider/taller
            // than the bottom-left / top-right bounds and causes edge breaking while scrolling.
            return Mathf.Min(maxZoomWidth, maxZoomHeight);
        }

        private void OnDrawGizmos()
        {
            MenuZoomBoundery();
            GameplayZoomBoundery();
            MapBoundery();
        }

        private void MenuZoomBoundery()
        {
            if(!m_UseMenuZoomBoundMethod) return;

            if(!m_ShowMenuZoomVisual) return;

            if (!m_MenuZoomLeftCenter || !m_MenuZoomRightCenter || !m_MenuZoomTopCenter || !m_MenuZoomBottomCenter)
            return;

            // ---------- Imaginary Boundary ----------
            Gizmos.color = m_MenuZoomBoundaryColor;

            Vector3 topLeft = new Vector3(m_MenuZoomLeftCenter.position.x, m_MenuZoomTopCenter.position.y, 0f);
            Vector3 topRight = new Vector3(m_MenuZoomRightCenter.position.x, m_MenuZoomTopCenter.position.y, 0f);
            Vector3 bottomRight = new Vector3(m_MenuZoomRightCenter.position.x, m_MenuZoomBottomCenter.position.y, 0f);
            Vector3 bottomLeft = new Vector3(m_MenuZoomLeftCenter.position.x, m_MenuZoomBottomCenter.position.y, 0f);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

             // ---------- Center Point ----------
            Vector3 center = (
                m_MenuZoomLeftCenter.position +
                m_MenuZoomRightCenter.position +
                m_MenuZoomTopCenter.position +
                m_MenuZoomBottomCenter.position
            ) / 4f;

            Gizmos.color = m_MenuZoomBoundaryColor;


            float size = 4f;

            // Draw cross
            Gizmos.DrawLine(center + Vector3.left * size, center + Vector3.right * size);
            Gizmos.DrawLine(center + Vector3.up * size, center + Vector3.down * size);

        }

        private void GameplayZoomBoundery()
        {
            if(!m_UseGameplayZoomBoundMethod) return;

            if(!m_ShowGameplayZoomVisual) return;

            if (!m_GameplayZoomLeftCenter || !m_GameplayZoomRightCenter || !m_GameplayZoomTopCenter || !m_GameplayZoomBottomCenter)
            return;

            // ---------- Imaginary Boundary ----------
            Gizmos.color = m_GameplayZoomBoundaryColor;

            Vector3 topLeft = new Vector3(m_GameplayZoomLeftCenter.position.x, m_GameplayZoomTopCenter.position.y, 0f);
            Vector3 topRight = new Vector3(m_GameplayZoomRightCenter.position.x, m_GameplayZoomTopCenter.position.y, 0f);
            Vector3 bottomRight = new Vector3(m_GameplayZoomRightCenter.position.x, m_GameplayZoomBottomCenter.position.y, 0f);
            Vector3 bottomLeft = new Vector3(m_GameplayZoomLeftCenter.position.x, m_GameplayZoomBottomCenter.position.y, 0f);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

            // ---------- Center Point ----------
            Vector3 center = (
                m_GameplayZoomLeftCenter.position +
                m_GameplayZoomRightCenter.position +
                m_GameplayZoomTopCenter.position +
                m_GameplayZoomBottomCenter.position
            ) / 4f;

            Gizmos.color = m_GameplayZoomBoundaryColor;

            float size = 4f;

            // Draw cross
            Gizmos.DrawLine(center + Vector3.left * size, center + Vector3.right * size);
            Gizmos.DrawLine(center + Vector3.up * size, center + Vector3.down * size);

        }

        private void MapBoundery()
        {
            if (m_MapBottomLeft == null || m_MapTopRight == null)
                return;

            Gizmos.color = Color.green;

            Vector3 bottomLeft = m_MapBottomLeft.position;
            Vector3 topRight = m_MapTopRight.position;

            Vector3 bottomRight = new Vector3(topRight.x, bottomLeft.y, bottomLeft.z);
            Vector3 topLeft = new Vector3(bottomLeft.x, topRight.y, bottomLeft.z);

            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }

        [ContextMenu("SetupForMenu")]
        private void CallSetupForMenu()
        {
            SetupForMenu(null);
        }
        [ContextMenu("SetupForGameplay")]
        private void CallSetupForGameplay()
        {
            SetupForGameplay(null);
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(CameraController) + " is null");
        }
    }
}
