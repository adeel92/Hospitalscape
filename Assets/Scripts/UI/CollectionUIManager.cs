using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using TMPro;

namespace Isometric.UI
{
    public class CollectionUIManager : MonoBehaviour
    {
        private static CollectionUIManager s_Instance;

        [SerializeField] GameObject m_Canvas;
        [SerializeField] bool m_IsDontDestroyOnLoad;


        [Header("---Curve Parameters---")]
        [SerializeField] float m_LeftCurveDisplacement;
        [SerializeField] float m_RightCurveDisplacment;
        [SerializeField] float m_UpCurveDisplacment;
        [SerializeField] float m_DownCurveDisplacment;
        [SerializeField] PathType m_PathType;

        [Header("---Curve Test (Call From ContextMeny)---")]
        [SerializeField] int m_CurveCount;
        [SerializeField] float m_CurveDuration;
        [SerializeField] CurveType m_CurveType;
        [SerializeField] GameObject m_CurveCollectable;
        [SerializeField] Transform m_CurveStartPosition;
        [SerializeField] Transform m_CurveEndPosition;
        private Collection m_CurveCollection = null;

        [Header("---Explosion Parameters---")]
        [SerializeField] Ease m_ExplosionEase = Ease.Linear;
        [SerializeField] Ease m_ExplosionCollectionEase = Ease.InOutQuad;

        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
                if (m_IsDontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        

        public static void Stop(Collection collection)
        {
            if (collection != null)
            {
                collection.Stop();
            }
        }

        #region Linear

        public static Collection CollectLinear(int count, float totalDuration, GameObject collectPrefab, Vector3 startPosition, Vector3 endPosition, bool timeScaleDependent, Action onCollect, Action onComplete)
        {
            if (s_Instance != null)
            {
                return AnimateLinearCollectables(count, totalDuration, collectPrefab, Vector3.one, Vector3.one, s_Instance.m_Canvas.transform, startPosition, endPosition, timeScaleDependent, onCollect, onComplete);
            }
            else
            {
                PrintInstanceNullError();
            }

            return null;
        }

        public static Collection CollectLinear(int count, 
            float totalDuration, 
            GameObject collectPrefab,
            Vector3 collectableStartSize,
            Vector3 collectableEndSize,
            Transform holder,
            Vector3 startPosition, 
            Vector3 endPosition,
            bool timeScaleDependent,
            Action onCollect, 
            Action onComplete)
        {
            if (s_Instance != null)
            {
                return AnimateLinearCollectables(count, totalDuration, collectPrefab, collectableStartSize, collectableEndSize, holder, startPosition, endPosition, timeScaleDependent, onCollect, onComplete);
            }
            else
            {
                PrintInstanceNullError();
            }

            return null;
        }

        /// <summary>
        /// Send collectPrefab and endPositions
        /// </summary>
        public static Collection CollectLinearMultiple(float totalDuration, List<Tuple<GameObject, Vector3>> collectPrefabs, Vector3 startPosition, Action onCollect, Action onComplete)
        {
            if (s_Instance != null)
            {
                return AnimateLinearCollectables(totalDuration, collectPrefabs, s_Instance.m_Canvas.transform, startPosition, onCollect, onComplete);
            }
            else
            {
                PrintInstanceNullError();
            }

            return null;
        }

        private static Collection AnimateLinearCollectables(
            int count,
            float totalDuration,
            GameObject collectPrefab,
            Vector3 collectableStartSize,
            Vector3 collectableEndSize,
            Transform canvas,
            Vector3 startPosition,
            Vector3 endPosition,
            bool timeScaleDependent,
            Action onCollect,
            Action onComplete)
        {
            if (count <= 0) count = 1;

            float delay = totalDuration / count;
            float duration = delay;
            float delayCounter = 0;

            Sequence sequence = DOTween.Sequence();
            List<GameObject> collectables = new List<GameObject>();

            for (int i = 0; i < count; i++)
            {
                GameObject collect = UnityEngine.Object.Instantiate(collectPrefab, canvas);
                collectables.Add(collect);
                collect.transform.position = startPosition;
                collect.transform.localScale = collectableStartSize;

                // Scale up and move
                sequence.Insert(delayCounter, collect.transform.DOScale(Vector3.one, duration * 0.1f));
                sequence.Insert(delayCounter, collect.transform.DOMove(endPosition, duration).SetEase(Ease.InOutQuad));
                sequence.Insert(delayCounter, collect.transform.DOScale(collectableEndSize, duration));

                // Shrink after move ends
                float shrinkStartTime = delayCounter + duration;
                sequence.Insert(shrinkStartTime, collect.transform.DOScale(Vector3.zero, duration * 0.1f));

                // Callback after shrink
                sequence.InsertCallback(shrinkStartTime + (duration * 0.1f), () =>
                {
                    if (collect != null)
                    {
                        collect.SetActive(false);
                    }

                    onCollect?.Invoke();
                });

                delayCounter += delay;
            }

            sequence.SetUpdate(!timeScaleDependent);

            Action OnCompleteClean = () =>
            {
                foreach (var collectable in collectables)
                {
                    if (collectable != null)
                    {
                        UnityEngine.Object.Destroy(collectable);
                    }
                }
            };


            Collection collection = new Collection(sequence, OnCompleteClean);
            
            sequence.OnComplete(() => { collection.IsStoped = true; OnCompleteClean?.Invoke(); onComplete?.Invoke(); });

            return collection;
        }



        private static Collection AnimateLinearCollectables(
            float totalDuration,
            List<Tuple<GameObject, Vector3>> collectPrefabs,
            Transform canvas,
            Vector3 startPosition,
            Action onCollect,
            Action onComplete)
        {
            int count = collectPrefabs?.Count ?? 0;
            if (count <= 0) return null;

            float duration = totalDuration;
            float shrinkDuration = duration * 0.1f;

            Sequence sequence = DOTween.Sequence();
            List<GameObject> collectables = new List<GameObject>();

            for (int i = 0; i < count; i++)
            {
                if (collectPrefabs[i] == null) continue;

                GameObject collect = Instantiate(collectPrefabs[i].Item1, canvas);
                collectables.Add(collect);
                collect.transform.position = startPosition;
                collect.transform.localScale = Vector3.zero;

                sequence.Insert(0, collect.transform.DOScale(Vector3.one, shrinkDuration));
                sequence.Insert(0, collect.transform.DOMove(collectPrefabs[i].Item2, duration).SetEase(Ease.InOutQuad));
                sequence.Insert(duration, collect.transform.DOScale(Vector3.zero, shrinkDuration));

                // Callback after shrink
                sequence.InsertCallback(duration + shrinkDuration, () =>
                {
                    if (collect != null)
                    {
                        collect.SetActive(false);
                    }

                    onCollect?.Invoke();
                });
            }

            Action OnCompleteClean = () =>
            {
                foreach (var collectable in collectables)
                {
                    if (collectable != null)
                    {
                        Destroy(collectable);
                    }
                }

                
            };

            Collection collection = new Collection(sequence, OnCompleteClean);

            sequence.OnComplete(() => { collection.IsStoped = true; onComplete?.Invoke(); OnCompleteClean?.Invoke(); });

            return collection;
        }



        #endregion

        #region Curve

        [ContextMenu("CollectCurveTest")]
        private void CollectCurveTest()
        {
            if (m_CurveCollection != null)
            {
                m_CurveCollection.Stop();
                m_CurveCollection = null;
            }
            CollectCurve(m_CurveCount, m_CurveDuration, m_CurveType, m_CurveCollectable, m_Canvas.transform, m_CurveStartPosition.position, m_CurveEndPosition.position, false, null, () => { m_CurveCollection = null; });
        }

        public static Collection CollectCurve(int count, float totalDuration, CurveType curveType, GameObject collectPrefab, Transform holder, Vector3 startPosition, Vector3 endPosition, bool isTimeScaleDependent, Action onCollect, Action onComplete)
        {
            if (s_Instance != null)
            {
                return AnimateCurveCollectables(count, totalDuration, curveType, collectPrefab, Vector3.one, Vector3.one, holder, startPosition, endPosition, isTimeScaleDependent, onCollect, onComplete);
            }
            else
            {
                PrintInstanceNullError();
            }

            return null;
        }

        public static Collection CollectCurve(int count, 
            float totalDuration, 
            CurveType curveType, 
            GameObject collectPrefab, 
            Vector3 collectableStartSize,
            Vector3 collectableEndSize, 
            Transform holder, 
            Vector3 startPosition, 
            Vector3 endPosition,
            bool isTimeScaleDependent,
            Action onCollect, 
            Action onComplete)
        {
            if (s_Instance != null)
            {
                return AnimateCurveCollectables(count, totalDuration, curveType, collectPrefab, collectableStartSize, collectableEndSize, holder, startPosition, endPosition, isTimeScaleDependent, onCollect, onComplete);
            }
            else
            {
                PrintInstanceNullError();
            }

            return null;
        }

        private static Collection AnimateCurveCollectables(
        int count,
        float totalDuration,
        CurveType curveType,
        GameObject collectPrefab,
        Vector3 collectableStartSize,
        Vector3 collectableEndSize,
        Transform canvas,
        Vector3 startPosition,
        Vector3 endPosition,
        bool timeScaleDependent,
        Action onCollect,
        Action onComplete)
        {
            if (count <= 0) count = 1;

            float delay = (totalDuration * 0.3f) / count;
            float duration = totalDuration * 0.7f;
            float delayCounter = 0;

            Sequence sequence = DOTween.Sequence();
            List<GameObject> collectables = new List<GameObject>();

            for (int i = 0; i < count; i++)
            {
                GameObject collect = Instantiate(collectPrefab, canvas);
                collectables.Add(collect);
                collect.transform.position = startPosition;
                collect.transform.localScale = collectableStartSize;

                // Build path points
                Vector3[] path = null;
                if (s_Instance.m_PathType == PathType.CubicBezier)
                {
                    path = GenerateCubicBezierPath(startPosition, endPosition, curveType);
                }
                else
                {
                    path = GenerateNormalCurvePath(startPosition, endPosition, curveType);
                }

                // Scale up first
                sequence.Insert(delayCounter, collect.transform.DOScale(Vector3.one, duration * 0.1f));

                // Path movement
                sequence.Insert(delayCounter, collect.transform.DOPath(path, duration, s_Instance.m_PathType, PathMode.Sidescroller2D)
                    .SetEase(Ease.InOutQuad));

                sequence.Insert(delayCounter, collect.transform.DOScale(collectableEndSize, duration).SetEase(Ease.Linear));

                // Shrink after reaching the end
                float shrinkStartTime = delayCounter + duration;
                sequence.Insert(shrinkStartTime, collect.transform.DOScale(Vector3.zero, duration * 0.1f));

                // Callback for individual collect
                sequence.InsertCallback(shrinkStartTime + (duration * 0.1f), () =>
                {
                    if (collect != null)
                        collect.SetActive(false);

                    onCollect?.Invoke();
                });

                delayCounter += delay;
            }

            sequence.SetUpdate(!timeScaleDependent);

            Action OnCompleteClean = () =>
            {
                foreach (var collectable in collectables)
                {
                    if (collectable != null)
                        UnityEngine.Object.Destroy(collectable);
                }
            };

            Collection collection = new Collection(sequence, OnCompleteClean);

            sequence.OnComplete(() => { collection.IsStoped = true; onComplete?.Invoke(); OnCompleteClean?.Invoke(); });

            return collection;
        }

        private static Vector3[] GenerateNormalCurvePath(Vector3 start, Vector3 end, CurveType curveType)
        {
            Vector3 displacmentLeft = start;
            displacmentLeft.x -= s_Instance.m_LeftCurveDisplacement;
            Vector3 displacmentRight = start;
            displacmentRight.x += s_Instance.m_RightCurveDisplacment;
            Vector3 displacmentDown = start;
            displacmentDown.y -= s_Instance.m_DownCurveDisplacment;
            Vector3 displacmentUp = start;
            displacmentUp.y += s_Instance.m_UpCurveDisplacment;

            List<Vector3> path = new List<Vector3> { start };
            if (s_Instance != null)
            {

                switch (curveType)
                {
                    case CurveType.CurveLeft:
                        path.Add(displacmentLeft);
                        break;
                    case CurveType.CurveRight:
                        path.Add(displacmentRight);
                        break;
                    case CurveType.CurveUp:
                        path.Add(displacmentUp);
                        break;
                    case CurveType.CurveDown:
                        path.Add(displacmentDown);
                        break;
                    case CurveType.CurveUpLeft:
                        path.Add(displacmentUp);
                        path.Add(displacmentLeft);
                        break;
                    case CurveType.CurveLeftUp:
                        path.Add(displacmentLeft);
                        path.Add(displacmentUp);
                        break;

                    case CurveType.CurveDownLeft:
                        path.Add(displacmentDown);
                        path.Add(displacmentLeft);
                        break;
                    case CurveType.CurveLeftDown:
                        path.Add(displacmentLeft);
                        path.Add(displacmentDown);
                        break;

                    case CurveType.CurveUpRight:
                        path.Add(displacmentUp);
                        path.Add(displacmentRight);
                        break;
                    case CurveType.CurveRightUp:
                        path.Add(displacmentRight);
                        path.Add(displacmentUp);
                        break;

                    case CurveType.CurveDownRight:
                        path.Add(displacmentDown);
                        path.Add(displacmentRight);
                        break;
                    case CurveType.CurveRightDown:
                        path.Add(displacmentRight);
                        path.Add(displacmentDown);
                        break;

                    case CurveType.CurveRightDownLeft:
                        path.Add(displacmentRight);
                        path.Add(displacmentDown);
                        path.Add(displacmentLeft);
                        break;

                    case CurveType.CurveDownLeftRight:
                        path.Add(displacmentDown);
                        path.Add(displacmentLeft);
                        path.Add(displacmentRight);
                        break;

                    case CurveType.CurveLeftRightDown:
                        path.Add(displacmentLeft);
                        path.Add(displacmentRight);
                        path.Add(displacmentDown);
                        break;
                }

                path.Add(end);
                return path.ToArray();
            }
            else
            {
                PrintInstanceNullError();
                return path.ToArray(); ;
            }
        }

        private static Vector3[] GenerateCubicBezierPath(Vector3 start, Vector3 end, CurveType curveType)
        {
            if (s_Instance == null)
            {
                PrintInstanceNullError();
                return new Vector3[] { end }; // fallback
            }

            List<Vector3> waypoints = new List<Vector3>();
            List<Vector3> intermediatePoints = new List<Vector3>();

            Vector3 displacmentLeft = start;
            displacmentLeft.x -= s_Instance.m_LeftCurveDisplacement;
            Vector3 displacmentRight = start;
            displacmentRight.x += s_Instance.m_RightCurveDisplacment;
            Vector3 displacmentDown = start;
            displacmentRight.y -= s_Instance.m_DownCurveDisplacment;
            Vector3 displacmentUp = start;
            displacmentRight.y += s_Instance.m_UpCurveDisplacment;

            // Handle all CurveTypes
            switch (curveType)
            {
                case CurveType.CurveLeft:
                    intermediatePoints.Add(displacmentLeft);
                    break;
                case CurveType.CurveRight:
                    intermediatePoints.Add(displacmentRight);
                    break;
                case CurveType.CurveUp:
                    intermediatePoints.Add(displacmentUp);
                    break;
                case CurveType.CurveDown:
                    intermediatePoints.Add(displacmentDown);
                    break;
                case CurveType.CurveUpLeft:
                    intermediatePoints.Add(displacmentUp);
                    intermediatePoints.Add(displacmentLeft);
                    break;
                case CurveType.CurveLeftUp:
                    intermediatePoints.Add(displacmentLeft);
                    intermediatePoints.Add(displacmentUp);
                    break;

                case CurveType.CurveDownLeft:
                    intermediatePoints.Add(displacmentDown);
                    intermediatePoints.Add(displacmentLeft);
                    break;
                case CurveType.CurveLeftDown:
                    intermediatePoints.Add(displacmentLeft);
                    intermediatePoints.Add(displacmentDown);
                    break;

                case CurveType.CurveUpRight:
                    intermediatePoints.Add(displacmentUp);
                    intermediatePoints.Add(displacmentRight);
                    break;
                case CurveType.CurveRightUp:
                    intermediatePoints.Add(displacmentRight);
                    intermediatePoints.Add(displacmentUp);
                    break;

                case CurveType.CurveDownRight:
                    intermediatePoints.Add(displacmentDown);
                    intermediatePoints.Add(displacmentRight);
                    break;
                case CurveType.CurveRightDown:
                    intermediatePoints.Add(displacmentRight);
                    intermediatePoints.Add(displacmentDown);
                    break;

                case CurveType.CurveRightDownLeft:
                    intermediatePoints.Add(displacmentRight);
                    intermediatePoints.Add(displacmentDown);
                    intermediatePoints.Add(displacmentLeft);
                    break;

                case CurveType.CurveDownLeftRight:
                    intermediatePoints.Add(displacmentDown);
                    intermediatePoints.Add(displacmentLeft);
                    intermediatePoints.Add(displacmentRight);
                    break;

                case CurveType.CurveLeftRightDown:
                    intermediatePoints.Add(displacmentLeft);
                    intermediatePoints.Add(displacmentRight);
                    intermediatePoints.Add(displacmentDown);
                    break;
            }

            // Construct Bezier segments
            Vector3 lastPos = start;
            foreach (var point in intermediatePoints)
            {
                AddBezierSegment(waypoints, lastPos, point);
                lastPos = point;
            }

            // Final segment to end
            AddBezierSegment(waypoints, lastPos, end);

            return waypoints.ToArray();
        }

        private static void AddBezierSegment(List<Vector3> list, Vector3 from, Vector3 to)
        {
            Vector3 direction = (to - from).normalized;
            float distance = Vector3.Distance(from, to);

            // Create controls spaced 1/3 of the way in/out
            Vector3 controlOut = from + direction * (distance / 3f);
            Vector3 controlIn = to - direction * (distance / 3f);

            list.Add(to);         // Waypoint
            list.Add(controlIn);  // IN control point
            list.Add(controlOut); // OUT control point
        }


        #endregion

        #region Explosion
        public static Collection CollectExplosion(int count, float totalDuration, GameObject collectPrefab, Vector2 startPosition, Vector2 endPosition, float explosionRadius)
        {
            if (s_Instance != null)
            {
                return AnimateExplosionCollectables(count, totalDuration, collectPrefab, s_Instance.m_Canvas.transform, startPosition, endPosition, explosionRadius, null, null);
            }
            else
            {
                PrintInstanceNullError();
            }

            return null;
        }

        public static Collection CollectExplosion(int count, float totalDuration, GameObject collectPrefab, Vector2 startPosition, Vector2 endPosition, float explosionRadius, Action onCollect, Action onComplete)
        {
            if (s_Instance != null)
            {
                return AnimateExplosionCollectables(count, totalDuration, collectPrefab, s_Instance.m_Canvas.transform, startPosition, endPosition, explosionRadius, onCollect, onComplete);
            }
            else
            {
                PrintInstanceNullError();
            }

            return null;
        }
        private static Collection AnimateExplosionCollectables(
        int count,
        float totalDuration,
        GameObject collectPrefab,
        Transform canvas,
        Vector2 startPosition,
        Vector2 endPosition,
        float explosionRadius,
        Action onCollect,
        Action onComplete)
        {
            float duration = totalDuration / 3;
            float delayCounter = 0;
            float delay = duration / count;

            Sequence sequence = DOTween.Sequence();
            List<GameObject> collectables = new List<GameObject>();

            for (int i = 0; i < count; i++)
            {
                GameObject collect = Instantiate(collectPrefab, canvas);
                collectables.Add(collect);
                collect.transform.position = startPosition;
                collect.transform.localScale = Vector3.one;

                float angle = (360f * UnityEngine.Random.value) * Mathf.Deg2Rad;

                float chance = UnityEngine.Random.value;
                float radius = Mathf.Sqrt(chance) * explosionRadius;

                Vector2 offset = new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );
                Vector2 explosionPosition = startPosition + offset;


                sequence.Insert(0, collect.transform.DOMove(explosionPosition, duration * 0.6f).SetEase(s_Instance.m_ExplosionEase));

                sequence.Insert(delayCounter + duration, collect.transform.DOMove(endPosition, duration)
                    .SetEase(s_Instance.m_ExplosionCollectionEase)
                    .OnComplete(() => {
                    if (collect != null)
                    {
                        collect.SetActive(false);
                    }

                    onCollect?.Invoke();
                }));
                delayCounter += delay;
            }

            Action OnCompleteClean = () =>
            {
                foreach (var collectable in collectables)
                {
                    if (collectable != null)
                    {
                        Destroy(collectable);
                    }
                }
            };

            Collection collection = new Collection(sequence, OnCompleteClean);

            sequence.OnComplete(() => { collection.IsStoped = true; onComplete?.Invoke(); OnCompleteClean?.Invoke(); });

            return collection;
        }

        #endregion

        public static void StopACoroutine(Coroutine coroutine)
        {
            if (s_Instance != null)
            {
                s_Instance.StopCoroutine(coroutine);
            }
            else
            {
                PrintInstanceNullError();
            }
        }

        protected static void PrintInstanceNullError()
        {
            Debug.LogWarning(nameof(CollectionUIManager) + " Instance is null");
        }
    }

    public class Collection
    {
        private Sequence m_Sequence;
        private Action OnCompleteClean;
        public bool IsStoped;

        public Collection(Sequence sequence, Action onCompleteClean)
        {
            m_Sequence = sequence;
            OnCompleteClean = onCompleteClean;
            IsStoped = false;
        }

        public void Stop()
        {
            if (IsStoped == false)
            {
                OnCompleteClean?.Invoke();
                m_Sequence?.Kill();
                IsStoped = true;
            }
        }
    }

    public enum CurveType
    {
        CurveLeft, 
        CurveRight, 
        CurveUp, 
        CurveDown,
        CurveUpLeft,
        CurveLeftUp, 
        CurveDownLeft,
        CurveLeftDown, 
        CurveUpRight,
        CurveRightUp, 
        CurveDownRight,
        CurveRightDown,
        CurveRightDownLeft,
        CurveDownLeftRight,
        CurveLeftRightDown,
    }
}
