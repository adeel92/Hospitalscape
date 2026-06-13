using UnityEngine;
using UnityEngine.Events;

namespace Isometric
{
    public class SpriteButton : MonoBehaviour
    {
        public UnityEvent OnClick;

        void OnMouseUp()
        {
            OnClick?.Invoke();
        }
    }
}
