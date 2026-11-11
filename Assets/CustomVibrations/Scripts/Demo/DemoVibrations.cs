using UnityEngine;
using UnityEngine.UI;

namespace CustomVibrations
{
    public class DemoVibrations : MonoBehaviour
    {
        [SerializeField] GameObject portraitCanvasObj;
        [SerializeField] GameObject landscapeCanvasObj;
        [Space, SerializeField] Button demoVibrationButton;

        private void Awake()
        {
            portraitCanvasObj.SetActive(false);
            landscapeCanvasObj.SetActive(false);

            ScreenOrientation orientation = Screen.orientation;
            switch (orientation)
            {
                case ScreenOrientation.Portrait:
                case ScreenOrientation.PortraitUpsideDown:
                portraitCanvasObj.SetActive(true);
                break;

                case ScreenOrientation.LandscapeLeft:
                case ScreenOrientation.LandscapeRight:
                landscapeCanvasObj.SetActive(true);
                break;
            }

            if(demoVibrationButton != null)
            {
                demoVibrationButton.interactable = false;
                Invoke(nameof(EnableDemoButton), 3f);
            }
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