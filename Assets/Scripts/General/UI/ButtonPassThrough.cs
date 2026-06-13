using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Arc
{
    /// This is a special button when clicked on not only it is going to be triggered if any 
    /// UI trigger object is behind it is also going to be triggered e.g (Button etc)
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ButtonPassThrough : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent OnClick;

        private CanvasGroup canvasGroup;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            else
            {
                canvasGroup.blocksRaycasts = false;
            }

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                if (result.gameObject == gameObject)
                    continue;

                var button = result.gameObject.GetComponent<Button>();
                if (button != null && button.interactable)
                {
                    button.onClick.Invoke();
                    break;
                }

                /*var eventTrigger = result.gameObject.GetComponent<EventTrigger>();
                if (eventTrigger != null)
                {
                    var pointerClick = eventTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerClick);
                    if (pointerClick != null)
                    {
                        pointerClick.callback?.Invoke(eventData);
                    }

                    var pointerDown = eventTrigger.triggers.Find((x) => x.eventID == EventTriggerType.PointerDown);
                    if (pointerDown != null)
                    {
                        pointerDown.callback?.Invoke(eventData);
                    }
                }*/
            }

            /*PointerEventData newEventData = new PointerEventData(EventSystem.current)
            {
                position = eventData.position
            };

            List<RaycastResult> newResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(newEventData, newResults);

            foreach (var result in newResults)
            {
                if (result.gameObject == gameObject)
                    continue;

                // Send generic click event (works for Button, EventTrigger, or IPointerClickHandler)
                ExecuteEvents.Execute<IPointerClickHandler>(
                    result.gameObject,
                    newEventData,
                    ExecuteEvents.pointerClickHandler
                );

                break; // Only click the first valid one
            }*/

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
            }
        }
    }
}
