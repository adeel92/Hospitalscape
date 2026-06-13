using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CrispSpriteDemo
{
    public class CrispSpriteDemo1 : MonoBehaviour
    {
        void Awake()
        {
            var dropdownOptions = new List<Dropdown.OptionData>();

            foreach (var group in m_groups)
            {
                for (int i = 0; i < group.transform.childCount; ++i)
                {
                    m_gameObjects.Add(group.transform.GetChild(i).gameObject);
                }

                dropdownOptions.Add(new Dropdown.OptionData(group.name));
            }

            ShowGroup(0);

            m_groupDropdown.ClearOptions();
            m_groupDropdown.AddOptions(dropdownOptions);
            m_groupDropdown.onValueChanged.AddListener(OnDropDownValueChanged);
        }

        void OnDropDownValueChanged(int a)
        {
            ShowGroup(a);
        }

        void ShowGroup(int a)
        {
            for (int i = 0; i < m_groups.Count; ++i)
            {
                m_groups[i].SetActive(i == a);
            }
        }

        void Start()
        {
        }

        void Update()
        {
            foreach (var go in m_gameObjects)
            {
                float r = (m_rotateSlider.value - 0.5f) * -360;
                go.transform.localEulerAngles = new Vector3(0, 0, r);

                float s = (m_scaleSlider.value - 0.5f) * 2 + 1;
                go.transform.localScale = new Vector3(s, s, s);
            }
        }

        public List<GameObject> m_groups;
        public Slider m_scaleSlider;
        public Slider m_rotateSlider;
        public Dropdown m_groupDropdown;

        List<GameObject> m_gameObjects = new List<GameObject>();
    }
}
