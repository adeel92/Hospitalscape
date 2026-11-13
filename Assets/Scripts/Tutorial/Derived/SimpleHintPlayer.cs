using Isometric;
using Isometric.Tutorial;

public class SimpleHintPlayer : SimpleHintTarget
{
    private void OnEnable()
    {
        GlobalEventHolder.OnPlayerStartedMoving += HideHint;
    }

    private void OnDisable()
    {
        GlobalEventHolder.OnPlayerStartedMoving -= HideHint;
    }
}
