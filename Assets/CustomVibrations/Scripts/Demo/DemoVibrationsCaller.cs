using UnityEngine;
using UnityEngine.UI;

namespace CustomVibrations
{
    public class DemoVibrationsCaller : MonoBehaviour
    {
        [SerializeField] Button demoVibrationButton;

        private void Awake()
        {
            demoVibrationButton.interactable = false;
            Invoke(nameof(EnableDemoButton), 3f);
        }

        private void EnableDemoButton()
        {
            demoVibrationButton.interactable = true;
        }

        public void PlayDemoVibration()
        {
            VibrationManager.Play(HapticType.DemoHapticType);
        }
    }
}