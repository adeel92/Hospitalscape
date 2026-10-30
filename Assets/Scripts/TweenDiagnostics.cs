using DG.Tweening;
using UnityEngine;

public static class TweenDiagnostics
{
    public static void LogActiveTweens(string label)
    {
        Debug.Log($"--- {label} ---");

        var tweens = DOTween.PlayingTweens();

        if (tweens == null)
        {
            Debug.Log("No playing tweens.");
            return;
        }

        foreach (Tween tween in tweens)
        {
            if (tween == null)
                continue;

            Object target = tween.target as Object;

            Debug.Log(
                $"Tween: {tween.GetType().Name}, " +
                $"Target: {(target != null ? target.name : "NULL/DESTROYED")}, " +
                $"Active: {tween.active}, " +
                $"Playing: {tween.IsPlaying()}"
            );
        }
    }
}