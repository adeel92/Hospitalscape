using System.Collections.Generic;
using UnityEngine;

public class FloorComboTester : MonoBehaviour
{
    [System.Serializable]
    public class FloorCombo
    {
        public Sprite hospitalFloor;
        public Sprite bedAreaFloor;
    }

    [Header("Sprite Renderers")]
    [SerializeField] private SpriteRenderer hospitalFloorRenderer;
    [SerializeField] private SpriteRenderer bedAreaFloorRenderer;

    [Header("Combinations")]
    [SerializeField] private List<FloorCombo> combos = new();

    private int currentIndex;

    private void Start()
    {
        ApplyCurrentCombo();
    }

    public void NextCombo()
    {
        if (combos.Count == 0)
            return;

        currentIndex = (currentIndex + 1) % combos.Count;
        ApplyCurrentCombo();
    }

    public void PreviousCombo()
    {
        if (combos.Count == 0)
            return;

        currentIndex = (currentIndex - 1 + combos.Count) % combos.Count;
        ApplyCurrentCombo();
    }

    private void ApplyCurrentCombo()
    {
        if (combos.Count == 0)
            return;

        hospitalFloorRenderer.sprite = combos[currentIndex].hospitalFloor;
        bedAreaFloorRenderer.sprite = combos[currentIndex].bedAreaFloor;
    }
}