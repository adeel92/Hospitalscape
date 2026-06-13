using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Isometric.Tutorial
{
    public class TutorialFocusManager : MonoBehaviour
    {
        private static TutorialFocusManager s_Instnace;

        [Serializable]
        private class FocusInfo
        {
            public FocusShapeType ShapeType;
            public Sprite ShapeSprite;
        }

        [SerializeField] GameObject m_Holder;
        [SerializeField] Button m_BackgroundButton;
        [SerializeField] Transform m_FocusHolder;
        //[SerializeField] Image m_FocusImage;
        [SerializeField] Image m_FocusImagePrefab;
        public List<Tuple<Transform, Coroutine>> m_Focuses = new List<Tuple<Transform, Coroutine>>();
        [SerializeField] List<FocusInfo> m_FocusInfos;


        private void Awake()
        {
            if (s_Instnace == null)
            {
                s_Instnace = this;
            }
        }

        public static Transform FocusAtPosition(Vector2 position, Vector2 offset, Vector2 scale, FocusShapeType shapeType)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return null;
            }

            s_Instnace.m_Holder.SetActive(true);

            Image focusImage = Instantiate(s_Instnace.m_FocusImagePrefab, s_Instnace.m_FocusHolder);

            Vector3 focusPosition = focusImage.transform.position;
            focusPosition.x = position.x + offset.x;
            focusPosition.y = position.y + offset.y;

            focusImage.transform.position = focusPosition;
            focusImage.transform.localScale = scale;

            FocusInfo focusInfo = s_Instnace.m_FocusInfos.Find((x) => x.ShapeType == shapeType);
            if (focusInfo != null)
            {
                focusImage.sprite = focusInfo.ShapeSprite;
            }


            Tuple<Transform, Coroutine> focus = new Tuple<Transform, Coroutine>(focusImage.transform, null);

            s_Instnace.m_Focuses.Add(focus);


            return focusImage.transform;
        }


        public static Transform FocusAtTransform(Transform target, Vector2 offset, Vector2 scale, FocusShapeType shapeType)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return null;
            }

            s_Instnace.m_Holder.SetActive(true);

            Image focusImage = Instantiate(s_Instnace.m_FocusImagePrefab, s_Instnace.m_FocusHolder);

            Vector3 focusPosition = focusImage.transform.position;
            focusPosition.x = target.position.x + offset.x;
            focusPosition.y = target.position.y + offset.y;

            focusImage.transform.position = focusPosition;
            focusImage.transform.localScale = scale;

            FocusInfo focusInfo = s_Instnace.m_FocusInfos.Find((x) => x.ShapeType == shapeType);
            if (focusInfo != null)
            {
                focusImage.sprite = focusInfo.ShapeSprite;
            }

            Coroutine focusCorotine = s_Instnace.StartCoroutine(s_Instnace.FocusingAtTransform(focusImage, target, offset));

            Tuple<Transform, Coroutine> focus = new Tuple<Transform, Coroutine>(focusImage.transform, focusCorotine);

            s_Instnace.m_Focuses.Add(focus);

            return focusImage.transform;
        }

        public static void SetBackgroundButtonCallback(Action backgroundClickCallback)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            s_Instnace.m_BackgroundButton.onClick.RemoveAllListeners();
            s_Instnace.m_BackgroundButton.onClick.AddListener(() => backgroundClickCallback?.Invoke());
        }

        public static void RemoveBackgroundButtonCallback()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }
            s_Instnace.m_BackgroundButton.onClick.RemoveAllListeners();
        }

        public static void StopFocus(Transform focusTarget)
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            var focus = s_Instnace.m_Focuses.Find((x) => x.Item1 == focusTarget);

            if (focus != null)
            {
                if (focus.Item1 != null)
                {
                    Destroy(focus.Item1.gameObject);
                }

                if (focus.Item2 != null)
                {
                    s_Instnace.StopCoroutine(focus.Item2);
                }

                s_Instnace.m_Focuses.Remove(focus);
            }

            if (s_Instnace.m_Focuses.Count == 0)
            {
                s_Instnace.m_Holder.SetActive(false);
            }
        }

        public static void StopAllFocus()
        {
            if (s_Instnace == null)
            {
                PrintNullInstanceError();
                return;
            }

            foreach (var focus in s_Instnace.m_Focuses)
            {
                if (focus.Item1 != null)
                {
                    Destroy(focus.Item1.gameObject);
                }

                if (focus.Item2 != null)
                {
                    s_Instnace.StopCoroutine(focus.Item2);
                }
            }

            s_Instnace.m_Focuses = new List<Tuple<Transform, Coroutine>>();
            s_Instnace.m_Holder.SetActive(false);
        }


        IEnumerator FocusingAtTransform(Image focusImage, Transform target, Vector2 offset)
        {
            while (true)
            {
                Vector3 focusPosition = focusImage.transform.position;
                focusPosition.x = target.position.x + offset.x;
                focusPosition.y = target.position.y + offset.y;

                focusImage.transform.position = focusPosition;

                yield return null;
            }
        }

        private static void PrintNullInstanceError()
        {
            Debug.LogWarning("Instance of " + nameof(TutorialFocusManager) + " is null");
        }
    }

    public enum FocusShapeType
    {
        Square, Circle, LongRectangle, ShortRectangle
    }
}
